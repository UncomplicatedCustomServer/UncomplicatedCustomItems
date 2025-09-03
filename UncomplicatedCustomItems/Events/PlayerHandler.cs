using CustomPlayerEffects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Wrappers;
using UncomplicatedCustomItems.Events.Methods;
using UnityEngine;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using UserSettings.ServerSpecific;
using static InventorySystem.Items.Firearms.Modules.DisruptorActionModule;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.Integrations;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.Events
{
    public class PlayerHandler
    {
        internal static Dictionary<Player, CoroutineHandle> _relativePosCoroutine = [];
        internal static Dictionary<Player, CoroutineHandle> _humeShieldRegenCoroutine = [];
        internal static Dictionary<int, CapybaraToy> _capybaras = [];
        internal static Dictionary<Player, long> _damageTimes = [];
        internal static Dictionary<PrimitiveObjectToy, int> _toolGunPrimitives = [];
        internal static readonly CachedLayerMask ToolGunMask = new("Default", "Door", "Glass");

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public static Dictionary<Pickup, Light> ActiveLights = [];
        /// <summary>
        /// The <see cref="Vector3"/> coordinates of the latest detonation point for a <see cref="ExplosiveGrenadeProjectile"/>.
        /// Triggered by the <see cref="GrenadeExploding"/> method.
        /// </summary>
        public static Vector3 DetonationPosition { get; set; } = Vector3.zero;

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles equipped keycards 
        /// Triggered by the <see cref="OnChangedItem"/> method.
        /// </summary>
        public static Dictionary<ushort, SummonedCustomItem> _equippedKeycards = [];


        public static void Register()
        {
            PlayerEvent.Hurt += OnHurt;
            PlayerEvent.TriggeringTesla += OnTriggeringTesla;
            PlayerEvent.ShootingWeapon += OnShooting;
            PlayerEvent.UsedItem += OnItemUse;
            PlayerEvent.DroppedItem += OnDrop;
            PlayerEvent.ShotWeapon += OnShot;
            PlayerEvent.UpdatingEffect += OnReceivingEffect;
            PlayerEvent.ThrewProjectile += OnThrownProjectile;
            PlayerEvent.Dying += OnDying;
            PlayerEvent.ChangedItem += OnChangedItem;
            PlayerEvent.DroppingItem += OnDropping;
            PlayerEvent.Hurting += OnHurting;
            PlayerEvent.InteractedDoor += OnDoorInteracting;
            PlayerEvent.UnlockingGenerator += OnGeneratorUnlock;
            PlayerEvent.InteractingLocker += OnLockerInteracting;
            PlayerEvent.Joined += OnVerified;
            PlayerEvent.PickedUpItem += OnPickup;
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.Left += OnLeft;
            PlayerEvent.FlippedCoin += OnFlippedCoin;
            PlayerEvent.ToggledFlashlight += OnToggledFlashlight;
            PlayerEvent.ToggledWeaponFlashlight += OnWeaponFlashlightToggled;
            PlayerEvent.ReloadingWeapon += OnReloading;
            PlayerEvent.ReloadedWeapon += OnReloaded;
            PlayerEvent.TogglingFlashlight += OnTogglingFlashlight;
            PlayerEvent.ThrowingProjectile += OnThrowingProjectile;
            PlayerEvent.ItemUsageEffectsApplying += OnUsingItemCompleted;
        }

        public static void Unregister()
        {
            PlayerEvent.Hurt -= OnHurt;
            PlayerEvent.TriggeringTesla -= OnTriggeringTesla;
            PlayerEvent.ShootingWeapon -= OnShooting;
            PlayerEvent.UsedItem -= OnItemUse;
            PlayerEvent.DroppedItem -= OnDrop;
            PlayerEvent.ShotWeapon -= OnShot;
            PlayerEvent.UpdatingEffect -= OnReceivingEffect;
            PlayerEvent.ThrewProjectile -= OnThrownProjectile;
            PlayerEvent.Dying -= OnDying;
            PlayerEvent.ChangedItem -= OnChangedItem;
            PlayerEvent.DroppingItem -= OnDropping;
            PlayerEvent.Hurting -= OnHurting;
            PlayerEvent.InteractedDoor -= OnDoorInteracting;
            PlayerEvent.UnlockingGenerator -= OnGeneratorUnlock;
            PlayerEvent.InteractingLocker -= OnLockerInteracting;
            PlayerEvent.Joined -= OnVerified;
            PlayerEvent.PickedUpItem -= OnPickup;
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.Left -= OnLeft;
            PlayerEvent.FlippedCoin -= OnFlippedCoin;
            PlayerEvent.ToggledFlashlight -= OnToggledFlashlight;
            PlayerEvent.ToggledWeaponFlashlight -= OnWeaponFlashlightToggled;
            PlayerEvent.ReloadingWeapon -= OnReloading;
            PlayerEvent.ReloadedWeapon -= OnReloaded;
            PlayerEvent.TogglingFlashlight -= OnTogglingFlashlight;
            PlayerEvent.ThrowingProjectile -= OnThrowingProjectile;
            PlayerEvent.ItemUsageEffectsApplying -= OnUsingItemCompleted;
        }
        
        public static void OnUsingItemCompleted(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (ev.UsableItem.Base is not Scp330Bag bag)
                return;

            int idx = bag.SelectedCandyId;
            if (idx < 0 || idx >= bag.Candies.Count)
                return;

            if (CandySerializationManager.TryGetCandyInBag(bag, idx, out SerializedCandy sc) && sc != null && sc.IsCustom)
            {
                if (Utilities.TryGetCustomItem(sc.CustomItemId, out ICustomItem iCustomItem) && iCustomItem.CustomData is ICandyData data)
                {
                    if (!data.ApplyEffects)
                    {
                        ev.ContinueProcess = true;
                        ev.IsAllowed = false;
                    }

                    ev.Player.SendHint(data.EatingMessage);

                    CustomItem customItem1 = iCustomItem as CustomItem;
                    if (customItem1.HasModule(CustomFlags.DieOnUse))
                    {
                        foreach (DieOnUseSettings dieOnUseSettings in customItem1.FlagSettings.DieOnUseSettings)
                        {
                            if (dieOnUseSettings.Vaporize ?? false)
                                ev.Player.Vaporize();

                            if (dieOnUseSettings.DeathMessage != null)
                                ev.Player.Kill($"{dieOnUseSettings.DeathMessage.Replace("%name%", customItem1.Name)}");
                            else
                                ev.Player.Kill($"Killed by {customItem1.Name}");
                        }
                    }

                    if (customItem1.HasModule(CustomFlags.EffectWhenUsed))
                    {
                        foreach (EffectSettings effectSettings in customItem1.FlagSettings.EffectSettings)
                        {
                            if (effectSettings.EffectEvent != null)
                            {
                                if (effectSettings.EffectEvent == "EffectWhenUsed")
                                {
                                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                                    {
                                        LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem1.Id} Name: {customItem1.Name}");
                                        return;
                                    }
                                    if (effectSettings.EffectDuration < -1)
                                    {
                                        LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem1.Id} Name: {customItem1.Name}");
                                        return;
                                    }
                                    if (effectSettings.EffectIntensity <= 0)
                                    {
                                        LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem1.Id} Name: {customItem1.Name}");
                                        return;
                                    }

                                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                                    string effect = effectSettings.Effect;
                                    float duration = effectSettings.EffectDuration;
                                    byte intensity = effectSettings.EffectIntensity;
                                    if (duration <= -1)
                                        ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                                    else
                                        ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                                }
                            }
                            else
                            {
                                LogManager.Error($"{nameof(OnItemUse)}: No FlagSettings found on {customItem1.Name}");
                            }
                        }
                    }

                    if (customItem1.HasModule(CustomFlags.CustomSound))
                    {
                        LogManager.Debug($"{nameof(OnItemUse)}: Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem1.Name}.");
                        API.Features.AudioSettings settings = customItem1.FlagSettings.AudioSettings.FirstOrDefault();
                        AudioApi.PlayAudio(settings.AudioPath, (float)settings.SoundVolume, ev.Player.Position, (float)settings.AudibleDistance);
                    }

                    if (customItem1.HasModule(CustomFlags.SwitchRoleOnUse))
                    {
                        foreach (SwitchRoleOnUseSettings SwitchRoleOnUseSettings in customItem1.FlagSettings.SwitchRoleOnUseSettings)
                        {
                            if (SwitchRoleOnUseSettings.RoleId == null || SwitchRoleOnUseSettings.RoleType == null || SwitchRoleOnUseSettings == null)
                                break;

                            if (SwitchRoleOnUseSettings.RoleType == "UCR")
                            {
                                if (UCR.TryGetCustomRole((int)SwitchRoleOnUseSettings.RoleId, out _))
                                {
                                    if (SwitchRoleOnUseSettings.Delay != null || SwitchRoleOnUseSettings.Delay > 0f)
                                    {
                                        Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                        {
                                            UCR.GiveCustomRole((int)SwitchRoleOnUseSettings.RoleId, ev.Player);
                                        });
                                    }
                                    else
                                    {
                                        UCR.GiveCustomRole((int)SwitchRoleOnUseSettings.RoleId, ev.Player);
                                    }
                                    if (SwitchRoleOnUseSettings.KeepLocation != null || SwitchRoleOnUseSettings.KeepLocation != false)
                                    {
                                        Vector3 OldPos = ev.Player.Position;
                                        Timing.CallDelayed(0.1f, () =>
                                        {
                                            ev.Player.Position = OldPos;
                                        });
                                    }
                                    break;
                                }
                                else
                                {
                                    LogManager.Warn($"{SwitchRoleOnUseSettings.RoleId} Is not a UCR role");
                                }
                            }
                            else if (SwitchRoleOnUseSettings.RoleType == "Normal")
                            {
                                if (ev.Player.Role != (RoleTypeId)SwitchRoleOnUseSettings.RoleId)
                                {
                                    if (SwitchRoleOnUseSettings.Delay != null || SwitchRoleOnUseSettings.Delay > 0f)
                                    {
                                        Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                        {
                                            ev.Player.SetRole((RoleTypeId)SwitchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)SwitchRoleOnUseSettings.SpawnFlags);
                                        });
                                    }
                                    else
                                    {
                                        ev.Player.SetRole((RoleTypeId)SwitchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)SwitchRoleOnUseSettings.SpawnFlags);
                                    }
                                    break;
                                }
                            }
                            else if (SwitchRoleOnUseSettings.RoleType != "UCR" || SwitchRoleOnUseSettings.RoleType != "Normal")
                            {
                                LogManager.Warn($"The role_type field in {customItem1.Name} is currently {SwitchRoleOnUseSettings.RoleType} and should be 'Normal', 'UCR', or 'ECR'");
                            }
                        }
                    }

                    if (customItem1.HasModule(CustomFlags.Capybara))
                    {
                        CapybaraToy capybara = CapybaraToy.Create(ev.Player.GameObject.transform);
                        capybara.CollidersEnabled = false;
                        capybara.Position += new Vector3(0, -0.8f, 0);
                        ev.Player.Scale = new(0.2f, 0.3f, 0.5f);
                        capybara.Scale = new(6f, 4f, 2.8f);
                        capybara.GameObject.name += "UCI";
                        _capybaras.TryAdd(ev.Player.PlayerId, capybara);
                    }
                }
            }
            else
            {
                LogManager.Info("Selected candy is not tracked as serialized.");
            }
        }

        public static void OnHurt(PlayerHurtEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker.CurrentItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem summonedCustomItem) || !summonedCustomItem.CustomItem.CustomFlags.HasValue)
                return;

            if (summonedCustomItem.HasModule(CustomFlags.LifeSteal))
            {
                foreach (LifeStealSettings lifeStealSettings in summonedCustomItem.CustomItem.FlagSettings.LifeStealSettings)
                {
                    if (Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem customItem) && customItem.CustomItem.CustomFlags.HasValue && customItem.HasModule(CustomFlags.LifeSteal))
                    {
                        LogManager.Debug("LifeSteal custom flag is being triggered");

                        if (lifeStealSettings != null)
                        {
                            float healedAmount = lifeStealSettings.LifeStealAmount * lifeStealSettings.LifeStealPercentage;
                            ev.Attacker.Heal(healedAmount);
                            LogManager.Debug($"LifeSteal custom flag triggered, healed {healedAmount} HP");
                        }
                    }
                }
            }
            if (summonedCustomItem.HasModule(CustomFlags.EffectShot))
            {
                foreach (EffectSettings effectSettings in summonedCustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Player.CurrentItem != null)
                    {
                        if (effectSettings.EffectEvent != null)
                        {
                            if (effectSettings.EffectEvent == "EffectShot")
                            {
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                                {
                                    LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (effectSettings.EffectDuration <= -2)
                                {
                                    LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (effectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                    return;
                                }

                                LogManager.Debug($"Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player.Nickname}");
                                string effect = effectSettings.Effect;
                                float duration = effectSettings.EffectDuration;
                                byte intensity = effectSettings.EffectIntensity;
                                ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {summonedCustomItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectShot Flag was triggered but couldnt be ran for {summonedCustomItem.CustomItem.Name}.");
                    }
                }
            }
        }

        public static void OnTriggeringTesla(PlayerTriggeringTeslaEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || !ev.IsAllowed)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.DoNotTriggerTeslaGates))
                ev.IsAllowed = false;
        }

        public static void OnReloading(PlayerReloadingWeaponEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.HasModule(CustomFlags.SingleFire))
            {
                if (customItem.MagazineModule.AmmoStored == 1)
                    ev.IsAllowed = false;
            }
        }

        public static void OnReloaded(PlayerReloadedWeaponEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.HasModule(CustomFlags.SingleFire))
            {
                customItem.MagazineModule.AmmoStored = 1;
            }
        }

        public static void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.FirearmItem == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (ev.FirearmItem.Base.TryGetModule(out IAdsModule module) && module.AdsTarget)
            {
                if (ev.FirearmItem.Type != ItemType.GunSCP127)
                {
                    IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                    customItem.HitscanHitregModule.BaseBulletInaccuracy = data.AimingInaccuracy;
                }
                else
                {
                    ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                    customItem.Scp127Hitscan.BaseBulletInaccuracy = data.AimingInaccuracy;
                }
            }
            else if (ev.FirearmItem.Type != ItemType.GunSCP127)
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                customItem.HitscanHitregModule.BaseBulletInaccuracy = data.Inaccuracy;
            }
            else
            {
                ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                customItem.Scp127Hitscan.BaseBulletInaccuracy = data.Inaccuracy;
            }

            if (!customItem.CustomItem.CustomFlags.HasValue || !customItem.HasModule(CustomFlags.None))
                return;

            if (customItem.HasModule(CustomFlags.InfiniteAmmo))
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                customItem.MagazineModule.AmmoStored = data.MaxMagazineAmmo;
                customItem.MagazineModule.ServerResyncData();
                LogManager.Silent($"InfiniteAmmo flag was triggered: magazine refilled to {data.MaxMagazineAmmo}");
            }
            if (customItem.HasModule(CustomFlags.CustomSound))
            {
                LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                AudioApi.PlayAudio(customItem, ev.Player.Position);
            }
            if (customItem.HasModule(CustomFlags.DieOnUse))
            {
                foreach (DieOnUseSettings dieOnUseSettings in customItem.CustomItem.FlagSettings.DieOnUseSettings)
                {
                    if (dieOnUseSettings.Vaporize ?? false)
                        ev.Player.Vaporize();

                    if (dieOnUseSettings.DeathMessage != null)
                        ev.Player.Kill($"{dieOnUseSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                    else
                        ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                }
                LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} killed.");
            }
        } 

        public static void OnItemUse(PlayerUsedItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.UsableItem == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.CustomItem.CustomData is ICandyData)
                return;

            if (customItem.HasModule(CustomFlags.DieOnUse))
            {
                foreach (DieOnUseSettings dieOnUseSettings in customItem.CustomItem.FlagSettings.DieOnUseSettings)
                {
                    if (dieOnUseSettings.Vaporize ?? false)
                        ev.Player.Vaporize();

                    if (dieOnUseSettings.DeathMessage != null)
                        ev.Player.Kill($"{dieOnUseSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                    else
                        ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                }
            }
            if (customItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {

                    if (effectSettings.EffectEvent != null)
                    {
                        if (effectSettings.EffectEvent == "EffectWhenUsed")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                            {
                                LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (effectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                            string effect = effectSettings.Effect;
                            float duration = effectSettings.EffectDuration;
                            byte intensity = effectSettings.EffectIntensity;
                            if (duration <= -1)
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                            else
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"{nameof(OnItemUse)}: No FlagSettings found on {customItem.CustomItem.Name}");
                    }
                }
            }
            if (customItem.HasModule(CustomFlags.CustomSound))
            {
                LogManager.Debug($"{nameof(OnItemUse)}: Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                AudioApi.PlayAudio(customItem, ev.Player.Position);
            }
            if (customItem.HasModule(CustomFlags.SwitchRoleOnUse))
                SwitchRoleOnUseMethod.Start(customItem, ev.Player);

            if (Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem item))
            {
                ISCP500Data scp500Data = item.CustomItem.CustomData as ISCP500Data;
                ISCP207Data scp207Data = item.CustomItem.CustomData as ISCP207Data;
                ISCP1853Data scp1853Data = item.CustomItem.CustomData as ISCP1853Data;
                ISCP1576Data scp1576Data = item.CustomItem.CustomData as ISCP1576Data;
                if (ev.UsableItem.Type == ItemType.SCP500)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == scp500Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {scp500Data.Effect} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp500Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {scp500Data.Duration} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp500Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {scp500Data.Intensity} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {scp500Data.Effect} at intensity {scp500Data.Intensity}, duration is {scp500Data.Duration} to {ev.Player.Nickname}");
                    string effect = scp500Data.Effect;
                    float duration = scp500Data.Duration;
                    byte intensity = scp500Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP207 || ev.UsableItem.Type == ItemType.AntiSCP207)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == scp207Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {scp207Data.Effect} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp207Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {scp207Data.Duration} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp207Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {scp207Data.Intensity} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {scp207Data.Effect} at intensity {scp207Data.Intensity}, duration is {scp207Data.Duration} to {ev.Player.Nickname}");
                    string effect = scp207Data.Effect;
                    float duration = scp207Data.Duration;
                    byte intensity = scp207Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP1853)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == scp1853Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {scp1853Data.Effect} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp1853Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {scp1853Data.Duration} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp1853Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {scp1853Data.Intensity} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {scp1853Data.Effect} at intensity {scp1853Data.Intensity}, duration is {scp1853Data.Duration} to {ev.Player.Nickname}");
                    string effect = scp1853Data.Effect;
                    float duration = scp1853Data.Duration;
                    byte intensity = scp1853Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP1576)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == scp1576Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {scp1576Data.Effect} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp1576Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {scp1576Data.Duration} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    if (scp1576Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {scp1576Data.Intensity} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {scp1576Data.Effect} at intensity {scp1576Data.Intensity}, duration is {scp1576Data.Duration} to {ev.Player.Nickname}");
                    string effect = scp1576Data.Effect;
                    float duration = scp1576Data.Duration;
                    byte intensity = scp1576Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP207 || ev.UsableItem.Type == ItemType.AntiSCP207)
                    if (scp207Data.RemoveItemAfterUse == false)
                        new SummonedCustomItem(item.CustomItem, ev.Player);
                if (ev.UsableItem.Type == ItemType.SCP1853)
                    if (scp1853Data.RemoveItemAfterUse == false)
                        new SummonedCustomItem(item.CustomItem, ev.Player);
                if (item.Item.Type == ItemType.Adrenaline || item.Item.Type == ItemType.Medkit || item.Item.Type == ItemType.Painkillers)
                    item.HandleCustomAction(item.Item);
            }
        }

        public static void OnChangedItem(PlayerChangedItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost)
                return;

            if (ev.OldItem is not null)
            {
                if (!Utilities.TryGetSummonedCustomItem(ev.OldItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                    return;

                if (customItem.HasModule(CustomFlags.EffectShot) || customItem.HasModule(CustomFlags.EffectWhenEquiped) || customItem.HasModule(CustomFlags.EffectWhenUsed))
                {
                    foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                    {
                        foreach (StatusEffectBase effect in ev.Player.ActiveEffects)
                        {
                            if (effect.name == effectSettings.Effect.ToString() && (bool)effectSettings.ClearOnUnequip)
                            {
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effectSettings.Effect, 0, 0);
                            }
                        }
                    }
                }
            }

            if (ev.NewItem is not null)
            {
                if (!Utilities.TryGetSummonedCustomItem(ev.NewItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                    return;

                if (customItem.CustomItem.Item == ItemType.GunSCP127 && customItem.CustomItem.CustomItemType == CustomItemType.SCPItem)
                {
                    ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(customItem.Item.Base);
                    StartHumeShieldRegen(ev.Player, data, tier, customItem);
                }

                if (customItem.HasModule(CustomFlags.EffectWhenEquiped))
                {
                    foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                    {
                        if (effectSettings.EffectEvent != null)
                        {
                            if (effectSettings.EffectEvent == "EffectWhenEquiped")
                            {
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                                {
                                    LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                if (effectSettings.EffectDuration < -1)
                                {
                                    LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                if (effectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }

                                LogManager.Debug($"{nameof(OnChangedItem)}: Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                                string effect = effectSettings.Effect;
                                float duration = effectSettings.EffectDuration;
                                byte intensity = effectSettings.EffectIntensity;
                                if (duration <= -1)
                                    ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                                else
                                    ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"{nameof(OnChangedItem)}: No FlagSettings found on {customItem.CustomItem.Name}");
                        }
                    }
                }
                if (customItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                    _equippedKeycards.TryAdd(customItem.Serial, customItem);
                if (customItem.HasModule(CustomFlags.ToolGun))
                {
                    ev.Player.GameObject.AddComponent<ToolGunUI>();
                }
            }
            if (ev.OldItem != null)
            {
                if (Utilities.TryGetSummonedCustomItem(ev.OldItem.Serial, out SummonedCustomItem customItem))
                    if (customItem.HasModule(CustomFlags.ToolGun))
                    {
                        SSTwoButtonsSetting clearList = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 23);
                        foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                            if (_toolGunPrimitives.TryGetValue(primitive, out int iD))
                                if (clearList.SyncIsA)
                                    if (ev.Player.PlayerId == iD)
                                        primitive.Destroy();
                    }
            }
        }

        internal static void StartHumeShieldRegen(Player player, ISCP127Data data, Scp127Tier tier, SummonedCustomItem customItem)
        {
            StopHumeShieldRegen(player);
            CoroutineHandle handle = Timing.RunCoroutine(HumeShieldRegeneration(player, data, tier, customItem));
            _humeShieldRegenCoroutine[player] = handle;
        }

        internal static void StopHumeShieldRegen(Player player)
        {
            if (_relativePosCoroutine.TryGetValue(player, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                _humeShieldRegenCoroutine.Remove(player);
            }
        }


        internal static IEnumerator<float> HumeShieldRegeneration(Player player, ISCP127Data data, Scp127Tier tier, SummonedCustomItem customItem)
        {
            float regenRate = 0f;
            float damagePause = 0f;

            switch (tier)
            {
                case Scp127Tier.Tier1:
                    regenRate = data.Tier1ShieldRegenRate;
                    damagePause = data.Tier1ShieldOnDamagePause;
                    break;
                case Scp127Tier.Tier2:
                    regenRate = data.Tier2ShieldRegenRate;
                    damagePause = data.Tier2ShieldOnDamagePause;
                    break;
                case Scp127Tier.Tier3:
                    regenRate = data.Tier3ShieldRegenRate;
                    damagePause = data.Tier3ShieldOnDamagePause;
                    break;
            }

            for (; ; )
            {
                if (_damageTimes.TryGetValue(player, out long time))
                {
                    long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
                    player.HumeShieldRegenRate = (elapsed >= damagePause) ? regenRate : 0f;
                }
                else
                {
                    player.HumeShieldRegenRate = regenRate;
                }

                if (player.CurrentItem == null || player.CurrentItem.Serial != customItem.Serial)
                    yield break;

                yield return Timing.WaitForOneFrame;
            }
        }

        public static void OnPickup(PlayerPickedUpItemEventArgs ev)
        {
            if (ev.Item == null)
                return;
            if (ev.Item.Category != ItemCategory.Armor)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.EffectWhenEquiped))
            {
                foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (effectSettings.EffectEvent != null)
                    {
                        if (effectSettings.EffectEvent == "EffectWhenEquiped")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                            {
                                LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (effectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                            string effect = effectSettings.Effect;
                            float duration = effectSettings.EffectDuration;
                            byte intensity = effectSettings.EffectIntensity;
                            if (duration <= -1)
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                            else
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                    }
                }
            }
            if (customItem.HasModule(CustomFlags.Capybara))
            {
                CapybaraToy capybara = CapybaraToy.Create(ev.Player.GameObject.transform);
                capybara.CollidersEnabled = false;
                capybara.Position += new Vector3(0, -0.8f, 0);
                ev.Player.Scale = new(0.2f, 0.3f, 0.5f);
                capybara.Scale = new(6f, 4f, 2.8f);
                capybara.GameObject.name += "UCI";
                _capybaras.Add(ev.Player.PlayerId, capybara);
            }
        }

        public static void OnHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;

            _damageTimes.TryAdd(ev.Player, DateTimeOffset.Now.ToUnixTimeMilliseconds());

            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out var customItem))
                return;

            if (customItem.CustomItem.CustomItemType == CustomItemType.MicroHID)
            {
                IMicroHIDData microData = customItem.CustomItem.CustomData as IMicroHIDData;
                MicroHidDamageHandler damageHandler = ev.DamageHandler as MicroHidDamageHandler;
                damageHandler.Damage = microData.Damage;
            }

            if (customItem.CustomItem.CustomItemType == CustomItemType.ParticleDisruptor)
            {
                IParticleDisruptorData disruptorData = customItem.CustomItem.CustomData as IParticleDisruptorData;
                DisruptorDamageHandler damageHandler = ev.DamageHandler as DisruptorDamageHandler;
                if (damageHandler.FiringState == FiringState.FiringSingle)
                    damageHandler.Damage = disruptorData.ChargeDamage;
                if (damageHandler.FiringState == FiringState.FiringRapid)
                    damageHandler.Damage = disruptorData.BurstDamage;
            }
        }

        public static void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.Role == RoleTypeId.Spectator || ev.Player.Role == RoleTypeId.Destroyed)
                return;

            CapybaraToy[] capybaras = [];
            capybaras = ev.Player.GameObject.GetComponents<CapybaraToy>();
            foreach (CapybaraToy toy in capybaras)
            {
                toy.Parent = null;
                toy.Position = new(1000, 1000, 1000);
                toy.Scale = new(0,0,0);
                toy.Destroy();
                ev.Player.Scale = new(1f, 1f, 1f);
            }

            Timing.CallDelayed(0.1f, () =>
            {
                foreach (Item item in ev.Player.Items)
                {
                    if (!Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                        return;

                    if (customItem.HasModule(CustomFlags.EffectWhenEquiped))
                    {
                        foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                        {
                            if (effectSettings.EffectEvent != null)
                            {
                                if (effectSettings.EffectEvent == "EffectWhenEquiped")
                                {
                                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                                    {
                                        LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                        return;
                                    }
                                    if (effectSettings.EffectDuration < -1)
                                    {
                                        LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                        return;
                                    }
                                    if (effectSettings.EffectIntensity <= 0)
                                    {
                                        LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                        return;
                                    }

                                    LogManager.Debug($"Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                                    string effect = effectSettings.Effect;
                                    float duration = effectSettings.EffectDuration;
                                    byte intensity = effectSettings.EffectIntensity;
                                    if (duration <= -1)
                                        ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                                    else
                                        ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                                }
                            }
                            else
                            {
                                LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                            }
                        }
                    }
                }
            });
        }

        public static void OnDoorInteracting(PlayerInteractedDoorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.CanOpen == false)
                return;
            if (ev.Door.Permissions == DoorPermissionFlags.None)
                return;
            if (ev.Player.CurrentItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
            {
                if (customItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData data = customItem.CustomItem.CustomData as IKeycardData;
                    if (ev.Door.Base.IsMoving && data.OneTimeUse)
                    {
                        Timing.CallDelayed(0.5f, () =>
                        {
                            ev.Player.SendHint($"{data.OneTimeUseHint.Replace("%name%", customItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {customItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(customItem.Item);
                            customItem.ResetBadge(ev.Player);
                        });
                    }
                }
            }
        }
        public static void OnGeneratorUnlock(PlayerUnlockingGeneratorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            if (ev.IsAllowed == false)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
            {
                if (customItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData data = customItem.CustomItem.CustomData as IKeycardData;
                    if (data.OneTimeUse)
                    {
                        Timing.CallDelayed(0.5f, () =>
                        {
                            ev.Player.SendHint($"{data.OneTimeUseHint.Replace("%name%", customItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {customItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(customItem.Item);
                        });
                    }
                }
            }
        }
        public static void OnLockerInteracting(PlayerInteractingLockerEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            if (ev.IsAllowed == false)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
            {
                if (customItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData data = customItem.CustomItem.CustomData as IKeycardData;
                    if (ev.Chamber.IsOpen && data.OneTimeUse)
                    {
                        Timing.CallDelayed(0.5f, () =>
                        {
                            ev.Player.SendHint($"{data.OneTimeUseHint.Replace("%name%", customItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {customItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(customItem.Item);
                        });
                    }
                }
            }
        }
        public static void OnWeaponFlashlightToggled(PlayerToggledWeaponFlashlightEventArgs ev)
        {
            if (ev.FirearmItem == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            customItem.FlashLightToggle = ev.NewState;
        }
        public static void OnFlippedCoin(PlayerFlippedCoinEventArgs ev)
        {
            if (ev.CoinItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.CoinItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.CustomItem.CustomItemType is CustomItemType.Item)
                customItem.HandleEvent(ev.Player, ItemEvents.Use, ev.CoinItem.Serial);

            if (customItem.HasModule(CustomFlags.SwitchRoleOnUse))
                SwitchRoleOnUseMethod.Start(customItem, ev.Player);

            if (customItem.HasModule(CustomFlags.CustomSound))
            {
                LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                AudioApi.PlayAudio(customItem, ev.Player.Position);
            }
        }
        public static void OnToggledFlashlight(PlayerToggledFlashlightEventArgs ev)
        {
            if (ev.LightItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.LightItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.CustomItem.CustomItemType is CustomItemType.Item)
                customItem.HandleEvent(ev.Player, ItemEvents.Use, ev.LightItem.Serial);

            if (customItem.HasModule(CustomFlags.SwitchRoleOnUse))
                SwitchRoleOnUseMethod.Start(customItem, ev.Player);

            if (customItem.HasModule(CustomFlags.CustomSound))
            {
                LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                AudioApi.PlayAudio(customItem, ev.Player.Position);
            }
        }

        public static void OnTogglingFlashlight(PlayerTogglingFlashlightEventArgs ev)
        {
            if (ev.LightItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.LightItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.CustomItem.CustomItemType is CustomItemType.Light)
            {
                ev.LightItem.IsEmitting = false;
                IFlashlightData data = customItem.CustomItem.CustomData as IFlashlightData;
                if (ev.NewState && customItem.Light.Intensity >= 0 && !customItem.Toggled)
                {
                    ev.IsAllowed = false;
                    ev.LightItem.IsEmitting = false;
                    customItem.Light.Intensity = data.Intensity;
                    customItem.Toggled = true;
                }
                else if (customItem.Toggled && customItem.Light.Intensity >= 1)
                {
                    ev.IsAllowed = false;
                    ev.LightItem.IsEmitting = false;
                    customItem.Toggled = false;
                    customItem.Light.Intensity = 0;
                }
            }
        }

        public static void OnThrownProjectile(PlayerThrewProjectileEventArgs ev)
        {
            if (ev.Projectile == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (effectSettings.EffectEvent != null)
                    {
                        if (effectSettings.EffectEvent == "EffectWhenUsed")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                            {
                                LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (effectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                            string effect = effectSettings.Effect;
                            float duration = effectSettings.EffectDuration;
                            byte intensity = effectSettings.EffectIntensity;
                            if (duration <= -1)
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                            else
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                    }
                }
            }
        }

        public static void OnDrop(PlayerDroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem summonedCustomItem))
                return;

            if (summonedCustomItem.HasModule(CustomFlags.Capybara))
            {
                CapybaraToy[] capybaras = [];
                capybaras = ev.Player.GameObject.GetComponents<CapybaraToy>();
                List<CapybaraToy> capybaras1 = capybaras.Where(c => c.GameObject.name.Contains("UCI")).ToList();
                foreach (CapybaraToy toy in capybaras1)
                {
                    toy.Parent = null;
                    toy.Position = new(1000, 1000, 1000);
                    toy.Destroy();
                    ev.Player.Scale = new(1f, 1f, 1f);
                }
            }
            if (summonedCustomItem.HasModule(CustomFlags.ToolGun))
            {
                ev.Pickup.Destroy();
                SummonedCustomItem.List.Remove(summonedCustomItem);
            }
            if (summonedCustomItem.HasModule(CustomFlags.EffectShot) || summonedCustomItem.HasModule(CustomFlags.EffectWhenEquiped) || summonedCustomItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings effectSettings in summonedCustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    foreach (StatusEffectBase effect in ev.Player.ActiveEffects)
                    {
                        if (effect.name == effectSettings.Effect && (bool)effectSettings.ClearOnUnequip)
                        {
                            ev.Player.ReferenceHub.playerEffectsController.ChangeState(effectSettings.Effect, 0, 0);
                        }
                    }
                }
            }
            if (summonedCustomItem.CustomItem.CustomFlags.HasValue && summonedCustomItem.HasModule(CustomFlags.DieOnDrop))
            {
                foreach (DieOnDropSettings dieOnDropSettings in summonedCustomItem.CustomItem.FlagSettings.DieOnDropSettings)
                {
                    LogManager.Debug($"Checking Vaporize setting for {summonedCustomItem.CustomItem.Name}");
                    if (dieOnDropSettings.Vaporize != null && (bool)dieOnDropSettings.Vaporize)
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Debug($"{ev.Player.Nickname} is being vaporized by {summonedCustomItem.CustomItem.Name}");
                            ev.Player.Vaporize();
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Vaporize {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                    else
                        LogManager.Debug($"Vaporize settings were null or false for {summonedCustomItem.CustomItem.Name}");
                    if (dieOnDropSettings.DeathMessage.Count() >= 1 && dieOnDropSettings.DeathMessage != null)
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                            ev.Player.Kill($"{dieOnDropSettings.DeathMessage.Replace("%name%", summonedCustomItem.CustomItem.Name)}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Kill {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                    else
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                            ev.Player.Kill($"Killed by {summonedCustomItem.CustomItem.Name}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Kill {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                }
            }
            else return;
        }

        public static void OnDropping(PlayerDroppingItemEventArgs ev)
        {
            if (ev.Item is null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;
            if (customItem.HasModule(CustomFlags.CantDrop))
            {
                ev.IsAllowed = false;
                foreach (CantDropSettings cantDropSettings in customItem.CustomItem.FlagSettings.CantDropSettings)
                {
                    if (cantDropSettings.HintOrBroadcast != null && cantDropSettings.HintOrBroadcast == "hint" || cantDropSettings.HintOrBroadcast == "Hint")
                    {
                        if (cantDropSettings.Message != null && cantDropSettings.Duration != null && cantDropSettings.Duration >= 1)
                        {
                            try
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Debug($"Sending CantDrop Hint to {ev.Player.Nickname}\nHint: {cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}");
                                ev.Player.SendHint($"{cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}", (ushort)cantDropSettings.Duration);
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Hint to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                            }
                        }
                    }
                    else if (cantDropSettings.HintOrBroadcast != null && cantDropSettings.HintOrBroadcast == "broadcast" || cantDropSettings.HintOrBroadcast == "Broadcast")
                    {
                        if (cantDropSettings.Message != null && cantDropSettings.Duration != null && cantDropSettings.Duration >= 1)
                        {
                            try
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.Nickname}\nBroadcast: {cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}");
                                ev.Player.SendBroadcast($"{cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}", (ushort)cantDropSettings.Duration, Broadcast.BroadcastFlags.Normal, true);
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Broadcast to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                            }
                        }
                    }
                    else
                    {
                        LogManager.Warn($"CantDropSettings HintOrBroadcast for {customItem.CustomItem.Name} is {cantDropSettings.HintOrBroadcast} Expected values are 'hint' or 'broadcast'");
                    }
                }
            }
            else return;
        }

        public static void OnDying(PlayerDyingEventArgs ev)
        {
            CapybaraToy[] capybaras = [];
            capybaras = ev.Player.GameObject.GetComponents<CapybaraToy>();
            foreach(CapybaraToy toy in capybaras)
            {
                toy.Parent = null;
                toy.Position = new(1000, 1000, 1000);
                toy.Scale = new(0, 0, 0);
                toy.Destroy();
                ev.Player.Scale = new(1f, 1f, 1f);
            }

            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (!ev.Attacker.Connection.isReady)
                return;
            if (!ev.Player.Connection.isReady)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;
            if (!ev.Attacker.CurrentItem.Type.IsWeapon())
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.VaporizeKills))
            {
                try
                {
                    LogManager.Silent("Name | Id | CustomFlag(s)");
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Debug($"Vaporizing {ev.Player.Nickname}");
                    ev.Player.Vaporize(ev.Attacker);
                }
                catch (Exception ex)
                {
                    LogManager.Silent("Name | Id | CustomFlag(s)");
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Error($"Couldnt Vaporize {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                }
            }
            if (customItem.HasModule(CustomFlags.HealOnKill))
            {
                foreach (HealOnKillSettings healOnKillSettings in customItem.CustomItem.FlagSettings.HealOnKillSettings)
                {
                    LogManager.Debug($"{nameof(OnDying)}: Healing {ev.Attacker.Nickname} by {healOnKillSettings.HealAmount}");
                    if (ev.Attacker.Health != ev.Attacker.MaxHealth)
                        ev.Attacker.Heal(healOnKillSettings.HealAmount ?? 5f);
                    else if (healOnKillSettings.ConvertToAhpIfFull ?? false)
                    {
                        LogManager.Debug($"{nameof(OnDying)}: {ev.Attacker.Nickname} health is full and ConvertToAhpIfFull is true adding {healOnKillSettings.HealAmount} to {ev.Attacker.Nickname} AHP");
                        ev.Attacker.ArtificialHealth = healOnKillSettings.HealAmount ?? 5f;
                    }
                }
            }
        }

        public static void OnThrowingProjectile(PlayerThrowingProjectileEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.ThrowableItem.Serial, out var item))
            {
                if (item.CustomItem.CustomItemType == CustomItemType.FlashGrenade && ev.ThrowableItem.Base.Projectile is FlashbangGrenade flashbang)
                {
                    IFlashGrenadeData flashGrenadeData = item.CustomItem.CustomData as IFlashGrenadeData;

                    flashbang.BlindTime = flashGrenadeData.MinimalDurationEffect;
                    flashbang._additionalBlurDuration = flashGrenadeData.AdditionalBlindedEffect;
                    flashbang._surfaceZoneDistanceIntensifier = flashGrenadeData.SurfaceDistanceIntensifier;
                    flashbang._fuseTime = flashGrenadeData.FuseTime;
                }
                else if (item.CustomItem.CustomItemType == CustomItemType.ExplosiveGrenade && ev.ThrowableItem.Base.Projectile is ExplosionGrenade grenade)
                {
                    IExplosiveGrenadeData explosiveGrenadeData = item.CustomItem.CustomData as IExplosiveGrenadeData;

                    grenade.MaxRadius = explosiveGrenadeData.MaxRadius;
                    grenade.ScpDamageMultiplier = explosiveGrenadeData.ScpDamageMultiplier;
                    grenade._concussedDuration = explosiveGrenadeData.ConcussDuration;
                    grenade._burnedDuration = explosiveGrenadeData.BurnDuration;
                    grenade._deafenedDuration = explosiveGrenadeData.DeafenDuration;
                    grenade._fuseTime = explosiveGrenadeData.FuseTime;
                }
            }
        }

        public static void OnVerified(PlayerJoinedEventArgs ev)
        {
            if (BadgeManager.devBadges.ContainsKey(ev.Player.UserId) && Plugin.Instance.Config.AllowDevPermissions)
            {
                LogManager.Debug($"Applying developer usergroup to {ev.Player.DisplayName} - {ev.Player.UserId} - {ev.Player.PlayerId}");
                LogManager.Security($"Allow Dev Permissions is enabled in your config! Any UCI developers can run commands on your server. If this was not intended, please disable it.");
                var (badgeText, badgeColor) = BadgeManager.devBadges[ev.Player.UserId];
                UserGroup userGroup = new()
                {
                    Permissions = ulong.MaxValue,
                    BadgeColor = badgeColor,
                    BadgeText = badgeText
                };

                ev.Player.UserGroup = userGroup;
                LogManager.Debug($"Developer usergroup applied to {ev.Player.DisplayName} - {ev.Player.UserId} - {ev.Player.PlayerId}");
            }
            else if (BadgeManager.devBadges.ContainsKey(ev.Player.UserId) && Plugin.Instance.Config.EnableCreditTags)
            {
                LogManager.Debug($"Applying developer badge to {ev.Player.DisplayName} - {ev.Player.UserId} - {ev.Player.PlayerId}");
                var (badgeText, badgeColor) = BadgeManager.devBadges[ev.Player.UserId];

                ev.Player.GroupName = badgeText;
                ev.Player.GroupColor = badgeColor;
                LogManager.Debug($"Developer badge applied to {ev.Player.DisplayName} - {ev.Player.UserId} - {ev.Player.PlayerId}");
            }
        }
        
        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.IsHost)
                return;

            if (_capybaras.ContainsKey(ev.Player.PlayerId))
                _capybaras.TryRemove(ev.Player.PlayerId);
        }

        public static void OnShot(PlayerShotWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.FirearmItem != null)
                    {
                        if (effectSettings.EffectEvent != null)
                        {
                            LogManager.Debug($"Checking if {effectSettings.EffectEvent} = EffectWhenUsed");
                            if (effectSettings.EffectEvent == "EffectWhenUsed")
                            {
                                LogManager.Debug($"{effectSettings.EffectEvent} = EffectWhenUsed");
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                                {
                                    LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                if (effectSettings.EffectDuration <= -2)
                                {
                                    LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                if (effectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                LogManager.Debug($"Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
                                string effect = effectSettings.Effect;
                                float duration = effectSettings.EffectDuration;
                                byte intensity = effectSettings.EffectIntensity;
                                if (duration <= -1)
                                    ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, float.MaxValue, effectSettings.AddDurationIfActive ?? false);
                                else
                                    ev.Player.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectWhenUsed Flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                    }
                }
            }
            if (Physics.Raycast(ev.Player.Camera.position, ev.Player.Camera.forward, out RaycastHit hitInfo, customItem.HitscanHitregModule.DamageFalloffDistance + customItem.HitscanHitregModule.FullDamageDistance, ToolGunMask))
            {
                if (customItem.HasModule(CustomFlags.ExplosiveBullets))
                {
                    foreach (ExplosiveBulletsSettings explosiveBulletsSettings in customItem.CustomItem.FlagSettings.ExplosiveBulletsSettings)
                    {
                        ExplosiveGrenadeProjectile grenade = (ExplosiveGrenadeProjectile)TimedGrenadeProjectile.SpawnActive(hitInfo.point, ItemType.GrenadeHE, ev.Player, 0.2);
                        grenade.MaxRadius = explosiveBulletsSettings.DamageRadius ?? 10f;
                        grenade.FuseEnd();
                    }
                }
                if (customItem.HasModule(CustomFlags.ToolGun))
                {
                    SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 22);
                    if (deletionMode.SyncIsA && ev.FirearmItem.IsAiming())
                    {
                        foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                        {
                            if (primitive.GameObject.name.Contains("UCI"))
                            {
                                Vector3 halfSize = primitive.Scale / 2f;
                                Vector3 minBounds = primitive.Position - halfSize;
                                Vector3 maxBounds = primitive.Position + halfSize;

                                if (hitInfo.point.x >= minBounds.x && hitInfo.point.x <= maxBounds.x && hitInfo.point.y >= minBounds.y && hitInfo.point.y <= maxBounds.y && hitInfo.point.z >= minBounds.z && hitInfo.point.z <= maxBounds.z)
                                    primitive.Destroy();
                            }
                        }
                    }
                    else if (deletionMode.SyncIsB && customItem.FlashLightToggle)
                    {
                        foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                        {
                            if (primitive.GameObject.name.Contains("UCI"))
                            {
                                Vector3 halfSize = primitive.Scale / 2f;
                                Vector3 minBounds = primitive.Position - halfSize;
                                Vector3 maxBounds = primitive.Position + halfSize;

                                if (hitInfo.point.x >= minBounds.x && hitInfo.point.x <= maxBounds.x && hitInfo.point.y >= minBounds.y && hitInfo.point.y <= maxBounds.y && hitInfo.point.z >= minBounds.z && hitInfo.point.z <= maxBounds.z)
                                    primitive.Destroy();
                            }
                        }
                    }
                    else
                    {
                        SSPlaintextSetting setting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(ev.Player.ReferenceHub, 21);
                        string room = string.Empty;
                        string[] components = setting.SyncInputText.Split(',');
                        Vector4 color = new();
                        if (components.Length == 4)
                        {
                            float x = float.Parse(components[0].Trim(), CultureInfo.InvariantCulture);
                            float y = float.Parse(components[1].Trim(), CultureInfo.InvariantCulture);
                            float z = float.Parse(components[2].Trim(), CultureInfo.InvariantCulture);
                            float w = float.Parse(components[3].Trim(), CultureInfo.InvariantCulture);

                            color = new Vector4(x, y, z, w);
                        }
                        if (ev.Player.Room.Name.ToString() != "Unnamed")
                            room = ev.Player.Room.Name.ToString();
                        else
                            room = ev.Player.Room.GameObject.name;
                        Vector3 relativePosition = ev.Player.Room.LocalPosition(hitInfo.point);
                        LogManager.Info($"Triggered by {ev.Player.Nickname}. Relative position inside {room}: {relativePosition}");
                        ev.Player.SendHint($"Relative position inside {room}: {relativePosition}. This was also sent to the console.", 6f);
                        ev.Player.SendConsoleMessage($"Relative position inside {room}: {relativePosition}", "white");
                        Vector3 scale = new(0.2f, 0.2f, 0.2f);
                        PrimitiveObjectToy primitive = PrimitiveObjectToy.Create(hitInfo.point);
                        primitive.Type = PrimitiveType.Cube;
                        primitive.Color = color;
                        primitive.Scale = scale;
                        primitive.Flags = AdminToys.PrimitiveFlags.Visible;
                        primitive.Rotation = ev.Player.Room.Rotation;
                        primitive.GameObject.name = $"UCI {relativePosition}";
                        _toolGunPrimitives.TryAdd(primitive, ev.Player.PlayerId);
                    }
                }
            }
        }

        public static void OnReceivingEffect(PlayerEffectUpdatingEventArgs ev)
        {
            if (ev.Effect == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
            {
                LogManager.Debug($"{ev.Player.Nickname} is reciving {ev.Effect}.");
                ISCP207Data scp207Data = customItem.CustomItem.CustomData as ISCP207Data;
                if (ev.Effect.GetType() == typeof(CustomPlayerEffects.Scp207) || ev.Effect.GetType() == typeof(CustomPlayerEffects.AntiScp207))
                {
                    LogManager.Debug("Effect is from a 207 custom item.");
                    if (scp207Data.Apply207Effect == false)
                    {
                        LogManager.Debug("Removing SCP-207 effect.");
                        ev.Intensity = 0;
                    }
                }
                ISCP1853Data scp1853Data = customItem.CustomItem.CustomData as ISCP1853Data;
                if (ev.Effect.GetType() == typeof(Scp1853))
                {
                    LogManager.Debug("Effect is from a 1853 custom item.");
                    if (scp1853Data.Apply1853Effect == false)
                    {
                        LogManager.Debug("Removing SCP-1853 effect.");
                        ev.Intensity = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Destroys the <see cref="Light"/> on a <see cref="CustomItem"/> <see cref="Pickup"/>.
        /// <param name="pickup"></param>
        /// </summary>
        public static void DestroyLightOnPickup(Pickup pickup)
        {
            if (Utilities.IsSummonedCustomItem(pickup.Serial))
            {
                LogManager.Debug($"{pickup.Type} is a Customitem");
                if (pickup == null || !ActiveLights.ContainsKey(pickup))
                    return;
                Light itemLight = ActiveLights[pickup];
                if (itemLight != null && itemLight.Base != null)
                {
                    itemLight.Destroy();
                    LogManager.Debug($"Destroyed light on {pickup.Type}");
                }
                ActiveLights.TryRemove(pickup);
                LogManager.Debug("Light successfully destroyed.");
            }
            else
            {
                return;
            }

        }
    }
}