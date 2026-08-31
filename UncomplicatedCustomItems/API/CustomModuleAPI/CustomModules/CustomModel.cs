using InventorySystem.Items.Thirdperson;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class CustomModel : CustomModuleBase
    {
        public override string Name => nameof(CustomModel);

        /// <summary>
        /// Gets or sets the name of the schematic to spawn (must be loaded in TME/MER).
        /// </summary>
        public string SchematicName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional animator name to target when the schematic has multiple animators.
        /// </summary>
        public string AnimatorName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the positional offset applied to the schematic relative to the held item / pickup.
        /// </summary>
        public Vector3 PositionOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Gets or sets the rotation offset applied to the schematic.
        /// </summary>
        public Vector3 RotationOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Gets or sets the scale of the schematic.
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.one;

        /// <summary>
        /// Gets or sets the animation to play for each <see cref="TriggerOn"/> event.
        /// </summary>
        public Dictionary<TriggerOn, string> Animations { get; set; } = [];

        [YamlIgnore]
        public object? Schematic { get; private set; }

        [YamlIgnore]
        public bool IsSpawned => Schematic != null;

        [YamlIgnore]
        private SummonedCustomItem? _item;

        [YamlIgnore]
        private CoroutineHandle _followHandle;

        [YamlIgnore]
        private int _lostAnchorFrames;

        [YamlIgnore]
        private Quaternion _rotationOffsetQuat;

        [YamlIgnore]
        private bool _applied;

        [YamlIgnore]
        private Vector3 _lastPosition;

        [YamlIgnore]
        private Vector3 _lastEuler;

        [YamlIgnore]
        private Vector3 _lastScale;

        [YamlIgnore]
        private float _retryDelay = 1f;

        [YamlIgnore]
        private bool _spawnFailLogged;

        [YamlIgnore]
        private Pickup? _pickupRef;

        [YamlIgnore]
        private bool _parented;

        private const int DespawnAfterFrames = 60;
        private const float PositionEpsilonSqr = 0.0004f;
        private const float RotationEpsilon = 0.05f;
        private const float ScaleEpsilon = 0.0001f;
        private const float MaxRetryDelay = 15f;

        private bool HasAnimations => Animations is { Count: > 0 };

        public override void OnAdded(SummonedCustomItem item)
        {
            _item = item;
            _rotationOffsetQuat = Quaternion.Euler(RotationOffset);
            base.OnAdded(item);

            if (string.IsNullOrWhiteSpace(SchematicName))
                return;

            _followHandle = Timing.RunCoroutine(FollowCoroutine());
        }

        public override void OnDestroyed()
        {
            StopFollow();

            if (Schematic != null)
            {
                MapEditorIntegration.DestroySchematic(Schematic);
                Schematic = null;
            }

            base.OnDestroyed();
        }

        public override void Run(EventArgs eventArgs)
        {
            if (Schematic == null || _item == null || !HasAnimations)
                return;

            TriggerOn trigger = GetTrigger(eventArgs);
            if (trigger == TriggerOn.None || !Check(eventArgs))
                return;

            if (Animations.TryGetValue(trigger, out string animationName) && !string.IsNullOrWhiteSpace(animationName))
                MapEditorIntegration.PlayAnimation(Schematic, animationName, AnimatorName);
        }

        public override void RegisterEvents()
        {
            if (!HasAnimations)
                return;

            PlayerEvents.ShotWeapon += Run;
            PlayerEvents.UsedItem += Run;
            PlayerEvents.ReloadedWeapon += Run;
            PlayerEvents.ChangedItem += Run;
            PlayerEvents.PickedUpItem += Run;
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.Death += Run;
            PlayerEvents.Hurt += Run;
            PlayerEvents.InteractedDoor += Run;
            PlayerEvents.InspectedItem += Run;
            ServerEvents.RoundEnding += Cleanup;
        }

        public override void UnregisterEvents()
        {
            if (!HasAnimations)
                return;

            PlayerEvents.ShotWeapon -= Run;
            PlayerEvents.UsedItem -= Run;
            PlayerEvents.ReloadedWeapon -= Run;
            PlayerEvents.ChangedItem -= Run;
            PlayerEvents.PickedUpItem -= Run;
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.Death -= Run;
            PlayerEvents.Hurt -= Run;
            PlayerEvents.InteractedDoor -= Run;
            PlayerEvents.InspectedItem -= Run;
            ServerEvents.RoundEnding -= Cleanup;
        }

        private void Cleanup(RoundEndingEventArgs ev)
        {
            Timing.KillCoroutines(_followHandle);

            if (Schematic != null)
                MapEditorIntegration.DestroySchematic(Schematic);
        }

        public void RemoveModel()
        {
            StopFollow();

            if (Schematic != null)
            {
                MapEditorIntegration.DestroySchematic(Schematic);
                Schematic = null;
                _lostAnchorFrames = 0;
            }
        }

        private void StopFollow()
        {
            if (_followHandle.IsRunning)
                Timing.KillCoroutines(_followHandle);
        }

        private IEnumerator<float> FollowCoroutine()
        {
            while (_item != null)
            {
                if (string.IsNullOrWhiteSpace(SchematicName))
                    yield break;

                if (!MapEditorIntegration.FoundAny)
                {
                    yield return Timing.WaitForSeconds(1f);
                    continue;
                }

                float? wait = null;
                try
                {
                    if (Schematic != null && MapEditorIntegration.IsSchematicDestroyed(Schematic))
                    {
                        Schematic = null;
                        _pickupRef = null;
                        _parented = false;
                        _lostAnchorFrames = 0;
                        _applied = false;
                        continue;
                    }

                    if (_item.Pickup is { IsDestroyed: false } pickup)
                    {
                        if (Schematic == null)
                        {
                            if (TryGetPickupWorld(out Vector3 spawnPosition, out Quaternion spawnRotation))
                            {
                                Schematic = MapEditorIntegration.SpawnSchematic(SchematicName, spawnPosition + (spawnRotation * PositionOffset), (spawnRotation * _rotationOffsetQuat).eulerAngles, Scale);
                                if (Schematic == null)
                                {
                                    if (!_spawnFailLogged)
                                    {
                                        _spawnFailLogged = true;
                                        LogManager.Warn($"[CustomModel] Failed to spawn schematic '{SchematicName}'. Retrying...");
                                    }

                                    wait = _retryDelay;
                                    _retryDelay = Mathf.Min(_retryDelay * 2f, MaxRetryDelay);
                                }
                            }
                            else
                            {
                                wait = 0.5f;
                            }
                        }

                        if (Schematic != null && !MapEditorIntegration.IsSchematicDestroyed(Schematic))
                        {
                            if (MapEditorIntegration.CanParent(Schematic))
                            {
                                if (_pickupRef != pickup)
                                {
                                    MapEditorIntegration.ParentTo(Schematic, pickup.Base.transform, PositionOffset, _rotationOffsetQuat, Scale);
                                    _pickupRef = pickup;
                                    _parented = true;
                                    _applied = false;
                                    _spawnFailLogged = false;
                                    _retryDelay = 1f;
                                    PlayAnimation(TriggerOn.OnAdded);
                                }
                            }
                            else
                            {
                                _lostAnchorFrames = 0;
                                UpdateTransform(pickup.Position, pickup.Rotation);
                            }
                        }
                    }
                    else
                    {
                        if (_parented && Schematic != null)
                        {
                            MapEditorIntegration.Unparent(Schematic);
                            _parented = false;
                            _pickupRef = null;
                            _applied = false;
                        }

                        if (Schematic == null)
                        {
                            _lostAnchorFrames = 0;

                            if (!TryGetAnchor(out Vector3 position, out Quaternion rotation))
                            {
                                wait = 0.5f;
                            }
                            else
                            {
                                Vector3 euler = (rotation * _rotationOffsetQuat).eulerAngles;
                                Schematic = MapEditorIntegration.SpawnSchematic(SchematicName, position + (rotation * PositionOffset), euler, Scale);
                                if (Schematic == null)
                                {
                                    if (!_spawnFailLogged)
                                    {
                                        _spawnFailLogged = true;
                                        LogManager.Warn($"[CustomModel] Failed to spawn schematic '{SchematicName}'. Retrying...");
                                    }

                                    wait = _retryDelay;
                                    _retryDelay = Mathf.Min(_retryDelay * 2f, MaxRetryDelay);
                                }
                                else
                                {
                                    _spawnFailLogged = false;
                                    _retryDelay = 1f;
                                    _applied = true;
                                    _lastPosition = position + (rotation * PositionOffset);
                                    _lastEuler = euler;
                                    _lastScale = Scale;
                                    PlayAnimation(TriggerOn.OnAdded);
                                }
                            }
                        }
                        else if (TryGetAnchor(out Vector3 anchorPosition, out Quaternion anchorRotation))
                        {
                            _lostAnchorFrames = 0;
                            UpdateTransform(anchorPosition, anchorRotation);
                        }
                        else if (++_lostAnchorFrames >= DespawnAfterFrames)
                        {
                            MapEditorIntegration.DestroySchematic(Schematic);
                            Schematic = null;
                            _lostAnchorFrames = 0;
                            _applied = false;
                            wait = 0.5f;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Warn($"[CustomModel] Follow coroutine error for '{SchematicName}': {ex.Message}");
                }

                if (wait.HasValue)
                {
                    yield return Timing.WaitForSeconds(wait.Value);
                    continue;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private bool TryGetPickupWorld(out Vector3 position, out Quaternion rotation)
        {
            if (_item?.Pickup is { IsDestroyed: false } pickup && pickup.Base != null)
            {
                position = pickup.Base.transform.position;
                rotation = pickup.Base.transform.rotation;
                return true;
            }

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        private void UpdateTransform(Vector3 anchorPosition, Quaternion anchorRotation)
        {
            if (Schematic == null)
                return;

            Vector3 position = anchorPosition + (anchorRotation * PositionOffset);
            Vector3 euler = (anchorRotation * _rotationOffsetQuat).eulerAngles;
            Vector3 scale = Scale;

            if (!_applied || (position - _lastPosition).sqrMagnitude >= PositionEpsilonSqr)
            {
                MapEditorIntegration.SetPosition(Schematic, position);
                _lastPosition = position;
            }

            if (!_applied || Vector3.Distance(euler, _lastEuler) >= RotationEpsilon)
            {
                MapEditorIntegration.SetRotation(Schematic, euler);
                _lastEuler = euler;
            }

            if (!_applied || Vector3.Distance(scale, _lastScale) >= ScaleEpsilon)
            {
                MapEditorIntegration.SetScale(Schematic, scale);
                _lastScale = scale;
            }

            _applied = true;
        }

        private void PlayAnimation(TriggerOn trigger)
        {
            if (Schematic == null)
                return;

            if (Animations.TryGetValue(trigger, out string animationName) && !string.IsNullOrWhiteSpace(animationName))
                MapEditorIntegration.PlayAnimation(Schematic, animationName, AnimatorName);
        }

        private bool TryGetAnchor(out Vector3 position, out Quaternion rotation)
        {
            if (_item == null)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            if (TryGetWorldModelTransform(out Transform? worldModel))
            {
                position = worldModel!.position;
                rotation = worldModel!.rotation;
                return true;
            }

            if (_item.Pickup != null && !_item.Pickup.IsDestroyed)
            {
                position = _item.Pickup.Position;
                rotation = _item.Pickup.Rotation;
                return true;
            }

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        private bool TryGetWorldModelTransform(out Transform? transform)
        {
            transform = null;

            if (_item?.Owner?.ReferenceHub?.roleManager?.CurrentRole is not IFpcRole fpcRole)
                return false;

            if (fpcRole.FpcModule?.CharacterModelInstance is not AnimatedCharacterModel characterModel)
                return false;

            if (!characterModel.TryGetSubcontroller<InventorySubcontroller>(out InventorySubcontroller subcontroller) || subcontroller == null)
                return false;

            if (!subcontroller.TryGetCurrentInstance(out ThirdpersonItemBase instance) || instance == null)
                return false;

            if (instance.ItemId.SerialNumber != _item.Serial)
                return false;

            transform = instance.transform;
            return transform != null;
        }

        private static TriggerOn GetTrigger(EventArgs eventArgs)
        {
            return eventArgs switch
            {
                PlayerShotWeaponEventArgs => TriggerOn.OnShot,
                PlayerUsedItemEventArgs => TriggerOn.OnUse,
                PlayerReloadedWeaponEventArgs => TriggerOn.OnReload,
                PlayerChangedItemEventArgs => TriggerOn.OnChangedItem,
                PlayerPickedUpItemEventArgs => TriggerOn.OnAdded,
                PlayerDroppedItemEventArgs => TriggerOn.OnDropped,
                PlayerDeathEventArgs => TriggerOn.OnDeath,
                PlayerHurtEventArgs => TriggerOn.OnHurt,
                PlayerInteractedDoorEventArgs => TriggerOn.OnDoorInteracted,
                PlayerInspectedItemEventArgs => TriggerOn.OnInspected,
                _ => TriggerOn.None
            };
        }
    }
}