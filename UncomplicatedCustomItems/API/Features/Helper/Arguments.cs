using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.Commands;
using UncomplicatedCustomItems.API.Enums;
using UnityEngine;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class Arguments
    {
        private static readonly Dictionary<Type, ArgumentType> EventTypeMapping = [];
        private static bool _isInitialized = false;

        public static void Register()
        {
            ArgumentManager.Register("ServerBroadcast", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: ServerBroadcast triggered.");
                string msg = string.Join(" ", args);
                Server.SendBroadcast(msg, 5);
            });

            ArgumentManager.Register("PlayerBroadcast", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: PlayerBroadcast triggered.");
                if (args == null || args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                string msg = string.Join(" ", args.Skip(1));
                if (string.IsNullOrWhiteSpace(msg)) return;

                player.SendBroadcast(msg, 5);
            });

            ArgumentManager.Register("GiveItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: GiveItem triggered.");
                if (args.Length < 2) return;
                string playerId = args[0];
                string itemName = args[1];
                Player player = Player.Get(playerId);
                if (player is null) return;

                player.AddItem((ItemType)Enum.Parse(typeof(ItemType), itemName, true));
            });

            ArgumentManager.Register("RemoveItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: RemoveItem triggered.");
                if (args.Length < 2) return;
                string playerId = args[0];
                string itemName = args[1];
                Player player = Player.Get(playerId);
                if (player is null) return;

                player.RemoveItem((ItemType)Enum.Parse(typeof(ItemType), itemName, true));
            });

            ArgumentManager.Register("GiveCustomItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: GiveCustomItem triggered.");
                if (args.Length < 2) return;
                string playerId = args[0];
                string customItem = args[1];
                Player player = Player.Get(playerId);
                if (player is null) return;
                if (!Utilities.TryGetCustomItemByName(customItem, out var customItem1)) return;

                new SummonedCustomItem(customItem1, player);
            });

            ArgumentManager.Register("Heal", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: Heal triggered.");
                if (args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (float.TryParse(args[1], out float heal))
                    player.Health += heal;

                LogManager.Debug($"{nameof(Arguments)}: {player.Health}/{player.MaxHealth}.");
            });

            ArgumentManager.Register("HealIfNotFull", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: HealIfNotFull triggered.");
                if (args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (float.TryParse(args[1], out float heal) && player.Health < player.MaxHealth)
                    player.Health += heal;

                LogManager.Debug($"{nameof(Arguments)}: {player.Health}/{player.MaxHealth}.");
            });

            ArgumentManager.Register("Cmd", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: Cmd triggered.");
                SilentCommandSender sender = new();
                string cmd = string.Join(" ", args);

                Server.RunCommand(cmd, sender);
                LogManager.Debug($"Ran command {cmd}");
            });

            ArgumentManager.Register("ApplyEffect", (item, args) =>
            {
                if (args.Length < 4) return;
                string playerId = args[0];
                string effectName = args[1];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectName) && float.TryParse(args[2], out float duration) && byte.TryParse(args[3], out byte intensity))
                {
                    player.ReferenceHub.playerEffectsController.ChangeState(effectName, intensity, duration, bool.Parse(args[4]));
                }
            });

            ArgumentManager.Register("RemoveEffect", (item, args) =>
            {
                if (args.Length < 2) return;
                string playerId = args[0];
                string effectName = args[1];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectName))
                {
                    player.ReferenceHub.playerEffectsController.ChangeState(effectName, 0, 0, false);
                }
            });

            ArgumentManager.Register("ClearEffects", (item, args) =>
            {
                if (args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player is null) return;

                player.DisableAllEffects();
            });

            ArgumentManager.Register("Delay", (item, args) =>
            {
                if (args.Length < 1) return;
                if (float.TryParse(args[0], out float seconds))
                {
                    Timing.CallDelayed(seconds, () =>
                    {
                        LogManager.Debug($"Delay of {seconds} seconds completed");
                    });
                }
            });

            ArgumentManager.Register("HealToMax", (item, args) =>
            {
                if (args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player is null) return;

                player.Health = player.MaxHealth;
            });

            ArgumentManager.Register("TeleportPlayer", (item, args) =>
            {
                if (args.Length < 4) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (float.TryParse(args[1], out float x) && float.TryParse(args[2], out float y) && float.TryParse(args[3], out float z))
                {
                    player.Position = new Vector3(x, y, z);
                }
            });

            ArgumentManager.Register("RandomTeleport", (item, args) =>
            {
                if (args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (player.RoleBase is IFpcRole fpcrole)
                    player.Position = Scp106PocketExitFinder.GetBestExitPosition(fpcrole);
            });

            ArgumentManager.Register("SetRole", (item, args) =>
            {
                if (args.Length < 2) return;
                string playerId = args[0];
                string roleName = args[1];
                Player player = Player.Get(playerId);
                if (player is null) return;

                if (Enum.TryParse<RoleTypeId>(roleName, true, out var role))
                    player.SetRole(role);
            });

            ArgumentManager.Register("SendHint", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: SendHint triggered.");
                if (args == null || args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                int duration = 5;
                duration = int.Parse(args[1]);
                string msg = string.Join(" ", args.Skip(2));
                if (string.IsNullOrWhiteSpace(msg)) return;

                player.SendHint(msg, duration);
            });

            ArgumentManager.Register("Kill", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: Kill triggered.");
                if (args == null || args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                string msg = string.Join(" ", args.Skip(1));

                player.Kill(msg);
            });

            ArgumentManager.Register("Damage", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: Damage triggered.");
                if (args == null || args.Length < 3) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                int amount = int.Parse(args[1]);
                string msg = string.Join(" ", args.Skip(2));

                player.Damage(amount, msg);
            });

            ArgumentManager.Register("ClearInventory", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: ClearInventory triggered.");
                if (args == null || args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;

                player.ClearItems();
            });

            ArgumentManager.Register("DropCurrentItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: DropCurrentItem triggered.");
                if (args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;

                player.CurrentItem?.DropItem();
            });

            ArgumentManager.Register("DropItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: DropItem triggered.");
                if (args == null || args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                string itemName = args[1];
                ItemType dropitemtype = (ItemType)Enum.Parse(typeof(ItemType), itemName, true);

                player.Items.Where(i => i.Type == dropitemtype).FirstOrDefault()?.DropItem();
            });

            ArgumentManager.Register("DestroyItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: DestroyItem triggered.");
                if (args == null || args.Length < 2) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                string itemName = args[1];
                ItemType destroyitemtype = (ItemType)Enum.Parse(typeof(ItemType), itemName, true);

                player.Items.Where(i => i.Type == destroyitemtype).FirstOrDefault()?.DropItem().Destroy();
            });

            ArgumentManager.Register("DestroyCurrentItem", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: DestroyCurrentItem triggered.");
                if (args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;

                player.CurrentItem?.DropItem().Destroy();
            });

            ArgumentManager.Register("Disarm", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: Disarm triggered.");
                if (args.Length < 1) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;

                if (!player.IsDisarmed)
                    player.IsDisarmed = true;
                else
                    player.IsDisarmed = false;
            });

            ArgumentManager.Register("PlayAudio", (item, args) =>
            {
                LogManager.Debug($"{nameof(Arguments)}: PlayAudio triggered.");
                if (args.Length < 4) return;
                string playerId = args[0];
                Player player = Player.Get(playerId);
                if (player == null) return;
                if (!float.TryParse(args[1], out float volume)) return;
                if (!float.TryParse(args[2], out float audibledistance)) return;
                string path = string.Join(" ", args.Skip(3));

                AudioApi.PlayAudio(path, volume, player.Position, audibledistance);
            });

            ArgumentManager.Register("action", (item, args) =>
            {
                LogManager.Debug($"{nameof(ArgumentManager)}: action triggered");
                if (args == null || args.Length == 0)
                {
                    LogManager.Error($"{nameof(ArgumentManager)}: Action command requires an action ID or name");
                    return;
                }

                string identifier = string.Join(" ", args);

                if (uint.TryParse(identifier, out uint actionId))
                {
                    if (CustomAction.CustomActions.TryGetValue(actionId, out ICustomAction customAction))
                    {
                        LogManager.Warn($"{nameof(ArgumentManager)}: Direct action handler called without EventArgs context. Use ExecuteCustomAction methods instead.");
                    }
                    else
                    {
                        LogManager.Error($"{nameof(ArgumentManager)}: CustomAction with ID {actionId} not found");
                    }
                    return;
                }

                ICustomAction foundAction = CustomAction.List.FirstOrDefault(a => string.Equals(a.Name, identifier, StringComparison.OrdinalIgnoreCase));

                if (foundAction != null)
                {
                    LogManager.Warn($"{nameof(ArgumentManager)}: Direct action handler called without EventArgs context. Use ExecuteCustomAction methods instead.");
                }
                else
                {
                    LogManager.Error($"{nameof(ArgumentManager)}: CustomAction not found: {identifier}");
                }
            });
        }

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            BuildEventMappings();
            RegisterEvents();
            _isInitialized = true;

            LogManager.Debug($"{nameof(Arguments)}: initialized with {EventTypeMapping.Count} event mappings.");
        }
        
        public static void Cleanup()
        {
            if (!_isInitialized)
                return;

            UnregisterEvents();
            EventTypeMapping.Clear();
            _isInitialized = false;

            LogManager.Debug($"{nameof(Arguments)}: cleaned up.");
        }

        private static void BuildEventMappings()
        {
            FieldInfo[] fields = typeof(ArgumentType).GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                EventTypeAttribute attribute = field.GetCustomAttribute<EventTypeAttribute>();
                if (attribute?.EventType != null)
                {
                    ArgumentType argumentType = (ArgumentType)field.GetValue(null);
                    EventTypeMapping[attribute.EventType] = argumentType;
                }
            }
        }

        private static void RegisterEvents()
        {
            PlayerEvent.Joined += OnPlayerJoined;
            PlayerEvent.Left += OnPlayerLeft;
            PlayerEvent.ReceivingVoiceMessage += OnReceivingVoiceMessage;
            PlayerEvent.SendingVoiceMessage += OnSendingVoiceMessage;
            PlayerEvent.PreAuthenticating += OnPreAuthenticating;
            PlayerEvent.PreAuthenticated += OnPreAuthenticated;
            PlayerEvent.UsingIntercom += OnUsingIntercom;
            PlayerEvent.UsedIntercom += OnUsedIntercom;
            PlayerEvent.Banning += OnBanning;
            PlayerEvent.Banned += OnBanned;
            PlayerEvent.Kicking += OnKicking;
            PlayerEvent.Kicked += OnKicked;
            PlayerEvent.Muting += OnMuting;
            PlayerEvent.Muted += OnMuted;
            PlayerEvent.Unmuting += OnUnmuting;
            PlayerEvent.Unmuted += OnUnmuted;
            PlayerEvent.ReportingCheater += OnReportingCheater;
            PlayerEvent.ReportedCheater += OnReportedCheater;
            PlayerEvent.ReportingPlayer += OnReportingPlayer;
            PlayerEvent.ReportedPlayer += OnReportedPlayer;
            PlayerEvent.TogglingNoclip += OnTogglingNoclip;
            PlayerEvent.ToggledNoclip += OnToggledNoclip;
            PlayerEvent.Jumped += OnJumped;
            PlayerEvent.MovementStateChanged += OnMovementStateChanged;
            PlayerEvent.RequestingRaPlayerList += OnRequestingRaPlayerList;
            PlayerEvent.RequestedRaPlayerList += OnRequestedRaPlayerList;
            PlayerEvent.RaPlayerListAddingPlayer += OnRaPlayerListAddingPlayer;
            PlayerEvent.RaPlayerListAddedPlayer += OnRaPlayerListAddedPlayer;
            PlayerEvent.RequestedCustomRaInfo += OnRequestedCustomRaInfo;
            PlayerEvent.RequestingRaPlayersInfo += OnRequestingRaPlayersInfo;
            PlayerEvent.RequestedRaPlayersInfo += OnRequestedRaPlayersInfo;
            PlayerEvent.RequestingRaPlayerInfo += OnRequestingRaPlayerInfo;
            PlayerEvent.RequestedRaPlayerInfo += OnRequestedRaPlayerInfo;
            PlayerEvent.ChangingBadgeVisibility += OnChangingBadgeVisibility;
            PlayerEvent.ChangedBadgeVisibility += OnChangedBadgeVisibility;
            PlayerEvent.ChangingNickname += OnChangingNickname;
            PlayerEvent.ChangedNickname += OnChangedNickname;
            PlayerEvent.GroupChanging += OnGroupChanging;
            PlayerEvent.GroupChanged += OnGroupChanged;
            PlayerEvent.UpdatingEffect += OnUpdatingEffect;
            PlayerEvent.UpdatedEffect += OnUpdatedEffect;
            PlayerEvent.Dying += OnDying;
            PlayerEvent.Death += OnDeath;
            PlayerEvent.Hurting += OnHurting;
            PlayerEvent.Hurt += OnHurt;
            PlayerEvent.ChangingRole += OnChangingRole;
            PlayerEvent.ChangedRole += OnChangedRole;
            PlayerEvent.Cuffing += OnCuffing;
            PlayerEvent.Cuffed += OnCuffed;
            PlayerEvent.Uncuffing += OnUncuffing;
            PlayerEvent.Uncuffed += OnUncuffed;
            PlayerEvent.ReceivingLoadout += OnReceivingLoadout;
            PlayerEvent.ReceivedLoadout += OnReceivedLoadout;
            PlayerEvent.Spawning += OnSpawning;
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.ChangingItem += OnChangingItem;
            PlayerEvent.ChangedItem += OnChangedItem;
            PlayerEvent.DroppingAmmo += OnDroppingAmmo;
            PlayerEvent.DroppedAmmo += OnDroppedAmmo;
            PlayerEvent.DroppingItem += OnDroppingItem;
            PlayerEvent.DroppedItem += OnDroppedItem;
            PlayerEvent.PickingUpAmmo += OnPickingUpAmmo;
            PlayerEvent.PickedUpAmmo += OnPickedUpAmmo;
            PlayerEvent.PickingUpArmor += OnPickingUpArmor;
            PlayerEvent.PickedUpArmor += OnPickedUpArmor;
            PlayerEvent.PickingUpItem += OnPickingUpItem;
            PlayerEvent.PickedUpItem += OnPickedUpItem;
            PlayerEvent.PickingUpScp330 += OnPickingUpScp330;
            PlayerEvent.PickedUpScp330 += OnPickedUpScp330;
            PlayerEvent.SearchedAmmo += OnSearchedAmmo;
            PlayerEvent.SearchingArmor += OnSearchingArmor;
            PlayerEvent.SearchedArmor += OnSearchedArmor;
            PlayerEvent.SearchingPickup += OnSearchingPickup;
            PlayerEvent.SearchedPickup += OnSearchedPickup;
            PlayerEvent.SearchingAmmo += OnSearchingAmmo;
            PlayerEvent.ThrowingItem += OnThrowingItem;
            PlayerEvent.ThrewItem += OnThrewItem;
            PlayerEvent.ThrowingProjectile += OnThrowingProjectile;
            PlayerEvent.ThrewProjectile += OnThrewProjectile;
            PlayerEvent.InspectingKeycard += OnInspectingKeycard;
            PlayerEvent.InspectedKeycard += OnInspectedKeycard;
            PlayerEvent.SpinningRevolver += OnSpinningRevolver;
            PlayerEvent.SpinnedRevolver += OnSpinnedRevolver;
            PlayerEvent.ToggledDisruptorFiringMode += OnToggledDisruptorFiringMode;
            PlayerEvent.UsingItem += OnUsingItem;
            PlayerEvent.UsedItem += OnUsedItem;
            PlayerEvent.ItemUsageEffectsApplying += OnItemUsageEffectsApplying;
            PlayerEvent.UsingRadio += OnUsingRadio;
            PlayerEvent.UsedRadio += OnUsedRadio;
            PlayerEvent.AimedWeapon += OnAimedWeapon;
            PlayerEvent.DryFiringWeapon += OnDryFiringWeapon;
            PlayerEvent.DryFiredWeapon += OnDryFiredWeapon;
            PlayerEvent.UnloadingWeapon += OnUnloadingWeapon;
            PlayerEvent.UnloadedWeapon += OnUnloadedWeapon;
            PlayerEvent.ReloadingWeapon += OnReloadingWeapon;
            PlayerEvent.ReloadedWeapon += OnReloadedWeapon;
            PlayerEvent.ShootingWeapon += OnShootingWeapon;
            PlayerEvent.ShotWeapon += OnShotWeapon;
            PlayerEvent.ChangingAttachments += OnChangingAttachments;
            PlayerEvent.ChangedAttachments += OnChangedAttachments;
            PlayerEvent.SendingAttachmentsPrefs += OnSendingAttachmentsPrefs;
            PlayerEvent.SentAttachmentsPrefs += OnSentAttachmentsPrefs;
            PlayerEvent.CancellingUsingItem += OnCancellingUsingItem;
            PlayerEvent.CancelledUsingItem += OnCancelledUsingItem;
            PlayerEvent.ChangingRadioRange += OnChangingRadioRange;
            PlayerEvent.ChangedRadioRange += OnChangedRadioRange;
            PlayerEvent.ProcessingJailbirdMessage += OnProcessingJailbirdMessage;
            PlayerEvent.ProcessedJailbirdMessage += OnProcessedJailbirdMessage;
            PlayerEvent.TogglingFlashlight += OnTogglingFlashlight;
            PlayerEvent.ToggledFlashlight += OnToggledFlashlight;
            PlayerEvent.TogglingWeaponFlashlight += OnTogglingWeaponFlashlight;
            PlayerEvent.ToggledWeaponFlashlight += OnToggledWeaponFlashlight;
            PlayerEvent.TogglingRadio += OnTogglingRadio;
            PlayerEvent.ToggledRadio += OnToggledRadio;
            PlayerEvent.DamagingShootingTarget += OnDamagingShootingTarget;
            PlayerEvent.DamagedShootingTarget += OnDamagedShootingTarget;
            PlayerEvent.DamagingWindow += OnDamagingWindow;
            PlayerEvent.DamagedWindow += OnDamagedWindow;
            PlayerEvent.EnteringPocketDimension += OnEnteringPocketDimension;
            PlayerEvent.EnteredPocketDimension += OnEnteredPocketDimension;
            PlayerEvent.LeavingPocketDimension += OnLeavingPocketDimension;
            PlayerEvent.LeftPocketDimension += OnLeftPocketDimension;
            PlayerEvent.TriggeringTesla += OnTriggeringTesla;
            PlayerEvent.TriggeredTesla += OnTriggeredTesla;
            PlayerEvent.Escaping += OnEscaping;
            PlayerEvent.Escaped += OnEscaped;
            PlayerEvent.FlippingCoin += OnFlippingCoin;
            PlayerEvent.FlippedCoin += OnFlippedCoin;
            PlayerEvent.SearchingToy += OnSearchingToy;
            PlayerEvent.SearchedToy += OnSearchedToy;
            PlayerEvent.SearchToyAborted += OnSearchToyAborted;
            PlayerEvent.IdlingTesla += OnIdlingTesla;
            PlayerEvent.IdledTesla += OnIdledTesla;
            PlayerEvent.InteractingDoor += OnInteractingDoor;
            PlayerEvent.InteractedDoor += OnInteractedDoor;
            PlayerEvent.InteractingElevator += OnInteractingElevator;
            PlayerEvent.InteractedElevator += OnInteractedElevator;
            PlayerEvent.InteractingGenerator += OnInteractingGenerator;
            PlayerEvent.InteractedGenerator += OnInteractedGenerator;
            PlayerEvent.OpeningGenerator += OnOpeningGenerator;
            PlayerEvent.OpenedGenerator += OnOpenedGenerator;
            PlayerEvent.ActivatingGenerator += OnActivatingGenerator;
            PlayerEvent.ActivatedGenerator += OnActivatedGenerator;
            PlayerEvent.DeactivatingGenerator += OnDeactivatingGenerator;
            PlayerEvent.DeactivatedGenerator += OnDeactivatedGenerator;
            PlayerEvent.UnlockingGenerator += OnUnlockingGenerator;
            PlayerEvent.UnlockedGenerator += OnUnlockedGenerator;
            PlayerEvent.ClosingGenerator += OnClosingGenerator;
            PlayerEvent.ClosedGenerator += OnClosedGenerator;
            PlayerEvent.InteractingLocker += OnInteractingLocker;
            PlayerEvent.InteractedLocker += OnInteractedLocker;
            PlayerEvent.InteractingScp330 += OnInteractingScp330;
            PlayerEvent.InteractedScp330 += OnInteractedScp330;
            PlayerEvent.InteractingShootingTarget += OnInteractingShootingTarget;
            PlayerEvent.InteractedShootingTarget += OnInteractedShootingTarget;
            PlayerEvent.PlacingBlood += OnPlacingBlood;
            PlayerEvent.PlacedBlood += OnPlacedBlood;
            PlayerEvent.PlacingBulletHole += OnPlacingBulletHole;
            PlayerEvent.PlacedBulletHole += OnPlacedBulletHole;
            PlayerEvent.SpawningRagdoll += OnSpawningRagdoll;
            PlayerEvent.SpawnedRagdoll += OnSpawnedRagdoll;
            PlayerEvent.UnlockingWarheadButton += OnUnlockingWarheadButton;
            PlayerEvent.UnlockedWarheadButton += OnUnlockedWarheadButton;
            PlayerEvent.ReceivedAchievement += OnReceivedAchievement;
            PlayerEvent.RoomChanged += OnRoomChanged;
            PlayerEvent.ZoneChanged += OnZoneChanged;
            PlayerEvent.InteractingWarheadLever += OnInteractingWarheadLever;
            PlayerEvent.InteractedWarheadLever += OnInteractedWarheadLever;
            PlayerEvent.ChangedSpectator += OnChangedSpectator;
            PlayerEvent.EnteringHazard += OnEnteringHazard;
            PlayerEvent.EnteredHazard += OnEnteredHazard;
            PlayerEvent.StayingInHazard += OnStayingInHazard;
            PlayerEvent.LeavingHazard += OnLeavingHazard;
            PlayerEvent.LeftHazard += OnLeftHazard;
        }

        private static void UnregisterEvents()
        {
            PlayerEvent.Joined -= OnPlayerJoined;
            PlayerEvent.Left -= OnPlayerLeft;
            PlayerEvent.ReceivingVoiceMessage -= OnReceivingVoiceMessage;
            PlayerEvent.SendingVoiceMessage -= OnSendingVoiceMessage;
            PlayerEvent.PreAuthenticating -= OnPreAuthenticating;
            PlayerEvent.PreAuthenticated -= OnPreAuthenticated;
            PlayerEvent.UsingIntercom -= OnUsingIntercom;
            PlayerEvent.UsedIntercom -= OnUsedIntercom;
            PlayerEvent.Banning -= OnBanning;
            PlayerEvent.Banned -= OnBanned;
            PlayerEvent.Kicking -= OnKicking;
            PlayerEvent.Kicked -= OnKicked;
            PlayerEvent.Muting -= OnMuting;
            PlayerEvent.Muted -= OnMuted;
            PlayerEvent.Unmuting -= OnUnmuting;
            PlayerEvent.Unmuted -= OnUnmuted;
            PlayerEvent.ReportingCheater -= OnReportingCheater;
            PlayerEvent.ReportedCheater -= OnReportedCheater;
            PlayerEvent.ReportingPlayer -= OnReportingPlayer;
            PlayerEvent.ReportedPlayer -= OnReportedPlayer;
            PlayerEvent.TogglingNoclip -= OnTogglingNoclip;
            PlayerEvent.ToggledNoclip -= OnToggledNoclip;
            PlayerEvent.Jumped -= OnJumped;
            PlayerEvent.MovementStateChanged -= OnMovementStateChanged;
            PlayerEvent.RequestingRaPlayerList -= OnRequestingRaPlayerList;
            PlayerEvent.RequestedRaPlayerList -= OnRequestedRaPlayerList;
            PlayerEvent.RaPlayerListAddingPlayer -= OnRaPlayerListAddingPlayer;
            PlayerEvent.RaPlayerListAddedPlayer -= OnRaPlayerListAddedPlayer;
            PlayerEvent.RequestedCustomRaInfo -= OnRequestedCustomRaInfo;
            PlayerEvent.RequestingRaPlayersInfo -= OnRequestingRaPlayersInfo;
            PlayerEvent.RequestedRaPlayersInfo -= OnRequestedRaPlayersInfo;
            PlayerEvent.RequestingRaPlayerInfo -= OnRequestingRaPlayerInfo;
            PlayerEvent.RequestedRaPlayerInfo -= OnRequestedRaPlayerInfo;
            PlayerEvent.ChangingBadgeVisibility -= OnChangingBadgeVisibility;
            PlayerEvent.ChangedBadgeVisibility -= OnChangedBadgeVisibility;
            PlayerEvent.ChangingNickname -= OnChangingNickname;
            PlayerEvent.ChangedNickname -= OnChangedNickname;
            PlayerEvent.GroupChanging -= OnGroupChanging;
            PlayerEvent.GroupChanged -= OnGroupChanged;
            PlayerEvent.UpdatingEffect -= OnUpdatingEffect;
            PlayerEvent.UpdatedEffect -= OnUpdatedEffect;
            PlayerEvent.Dying -= OnDying;
            PlayerEvent.Death -= OnDeath;
            PlayerEvent.Hurting -= OnHurting;
            PlayerEvent.Hurt -= OnHurt;
            PlayerEvent.ChangingRole -= OnChangingRole;
            PlayerEvent.ChangedRole -= OnChangedRole;
            PlayerEvent.Cuffing -= OnCuffing;
            PlayerEvent.Cuffed -= OnCuffed;
            PlayerEvent.Uncuffing -= OnUncuffing;
            PlayerEvent.Uncuffed -= OnUncuffed;
            PlayerEvent.ReceivingLoadout -= OnReceivingLoadout;
            PlayerEvent.ReceivedLoadout -= OnReceivedLoadout;
            PlayerEvent.Spawning -= OnSpawning;
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.ChangingItem -= OnChangingItem;
            PlayerEvent.ChangedItem -= OnChangedItem;
            PlayerEvent.DroppingAmmo -= OnDroppingAmmo;
            PlayerEvent.DroppedAmmo -= OnDroppedAmmo;
            PlayerEvent.DroppingItem -= OnDroppingItem;
            PlayerEvent.DroppedItem -= OnDroppedItem;
            PlayerEvent.PickingUpAmmo -= OnPickingUpAmmo;
            PlayerEvent.PickedUpAmmo -= OnPickedUpAmmo;
            PlayerEvent.PickingUpArmor -= OnPickingUpArmor;
            PlayerEvent.PickedUpArmor -= OnPickedUpArmor;
            PlayerEvent.PickingUpItem -= OnPickingUpItem;
            PlayerEvent.PickedUpItem -= OnPickedUpItem;
            PlayerEvent.PickingUpScp330 -= OnPickingUpScp330;
            PlayerEvent.PickedUpScp330 -= OnPickedUpScp330;
            PlayerEvent.SearchedAmmo -= OnSearchedAmmo;
            PlayerEvent.SearchingArmor -= OnSearchingArmor;
            PlayerEvent.SearchedArmor -= OnSearchedArmor;
            PlayerEvent.SearchingPickup -= OnSearchingPickup;
            PlayerEvent.SearchedPickup -= OnSearchedPickup;
            PlayerEvent.SearchingAmmo -= OnSearchingAmmo;
            PlayerEvent.ThrowingItem -= OnThrowingItem;
            PlayerEvent.ThrewItem -= OnThrewItem;
            PlayerEvent.ThrowingProjectile -= OnThrowingProjectile;
            PlayerEvent.ThrewProjectile -= OnThrewProjectile;
            PlayerEvent.InspectingKeycard -= OnInspectingKeycard;
            PlayerEvent.InspectedKeycard -= OnInspectedKeycard;
            PlayerEvent.SpinningRevolver -= OnSpinningRevolver;
            PlayerEvent.SpinnedRevolver -= OnSpinnedRevolver;
            PlayerEvent.ToggledDisruptorFiringMode -= OnToggledDisruptorFiringMode;
            PlayerEvent.UsingItem -= OnUsingItem;
            PlayerEvent.UsedItem -= OnUsedItem;
            PlayerEvent.ItemUsageEffectsApplying -= OnItemUsageEffectsApplying;
            PlayerEvent.UsingRadio -= OnUsingRadio;
            PlayerEvent.UsedRadio -= OnUsedRadio;
            PlayerEvent.AimedWeapon -= OnAimedWeapon;
            PlayerEvent.DryFiringWeapon -= OnDryFiringWeapon;
            PlayerEvent.DryFiredWeapon -= OnDryFiredWeapon;
            PlayerEvent.UnloadingWeapon -= OnUnloadingWeapon;
            PlayerEvent.UnloadedWeapon -= OnUnloadedWeapon;
            PlayerEvent.ReloadingWeapon -= OnReloadingWeapon;
            PlayerEvent.ReloadedWeapon -= OnReloadedWeapon;
            PlayerEvent.ShootingWeapon -= OnShootingWeapon;
            PlayerEvent.ShotWeapon -= OnShotWeapon;
            PlayerEvent.ChangingAttachments -= OnChangingAttachments;
            PlayerEvent.ChangedAttachments -= OnChangedAttachments;
            PlayerEvent.SendingAttachmentsPrefs -= OnSendingAttachmentsPrefs;
            PlayerEvent.SentAttachmentsPrefs -= OnSentAttachmentsPrefs;
            PlayerEvent.CancellingUsingItem -= OnCancellingUsingItem;
            PlayerEvent.CancelledUsingItem -= OnCancelledUsingItem;
            PlayerEvent.ChangingRadioRange -= OnChangingRadioRange;
            PlayerEvent.ChangedRadioRange -= OnChangedRadioRange;
            PlayerEvent.ProcessingJailbirdMessage -= OnProcessingJailbirdMessage;
            PlayerEvent.ProcessedJailbirdMessage -= OnProcessedJailbirdMessage;
            PlayerEvent.TogglingFlashlight -= OnTogglingFlashlight;
            PlayerEvent.ToggledFlashlight -= OnToggledFlashlight;
            PlayerEvent.TogglingWeaponFlashlight -= OnTogglingWeaponFlashlight;
            PlayerEvent.ToggledWeaponFlashlight -= OnToggledWeaponFlashlight;
            PlayerEvent.TogglingRadio -= OnTogglingRadio;
            PlayerEvent.ToggledRadio -= OnToggledRadio;
            PlayerEvent.DamagingShootingTarget -= OnDamagingShootingTarget;
            PlayerEvent.DamagedShootingTarget -= OnDamagedShootingTarget;
            PlayerEvent.DamagingWindow -= OnDamagingWindow;
            PlayerEvent.DamagedWindow -= OnDamagedWindow;
            PlayerEvent.EnteringPocketDimension -= OnEnteringPocketDimension;
            PlayerEvent.EnteredPocketDimension -= OnEnteredPocketDimension;
            PlayerEvent.LeavingPocketDimension -= OnLeavingPocketDimension;
            PlayerEvent.LeftPocketDimension -= OnLeftPocketDimension;
            PlayerEvent.TriggeringTesla -= OnTriggeringTesla;
            PlayerEvent.TriggeredTesla -= OnTriggeredTesla;
            PlayerEvent.Escaping -= OnEscaping;
            PlayerEvent.Escaped -= OnEscaped;
            PlayerEvent.FlippingCoin -= OnFlippingCoin;
            PlayerEvent.FlippedCoin -= OnFlippedCoin;
            PlayerEvent.SearchingToy -= OnSearchingToy;
            PlayerEvent.SearchedToy -= OnSearchedToy;
            PlayerEvent.SearchToyAborted -= OnSearchToyAborted;
            PlayerEvent.IdlingTesla -= OnIdlingTesla;
            PlayerEvent.IdledTesla -= OnIdledTesla;
            PlayerEvent.InteractingDoor -= OnInteractingDoor;
            PlayerEvent.InteractedDoor -= OnInteractedDoor;
            PlayerEvent.InteractingElevator -= OnInteractingElevator;
            PlayerEvent.InteractedElevator -= OnInteractedElevator;
            PlayerEvent.InteractingGenerator -= OnInteractingGenerator;
            PlayerEvent.InteractedGenerator -= OnInteractedGenerator;
            PlayerEvent.OpeningGenerator -= OnOpeningGenerator;
            PlayerEvent.OpenedGenerator -= OnOpenedGenerator;
            PlayerEvent.ActivatingGenerator -= OnActivatingGenerator;
            PlayerEvent.ActivatedGenerator -= OnActivatedGenerator;
            PlayerEvent.DeactivatingGenerator -= OnDeactivatingGenerator;
            PlayerEvent.DeactivatedGenerator -= OnDeactivatedGenerator;
            PlayerEvent.UnlockingGenerator -= OnUnlockingGenerator;
            PlayerEvent.UnlockedGenerator -= OnUnlockedGenerator;
            PlayerEvent.ClosingGenerator -= OnClosingGenerator;
            PlayerEvent.ClosedGenerator -= OnClosedGenerator;
            PlayerEvent.InteractingLocker -= OnInteractingLocker;
            PlayerEvent.InteractedLocker -= OnInteractedLocker;
            PlayerEvent.InteractingScp330 -= OnInteractingScp330;
            PlayerEvent.InteractedScp330 -= OnInteractedScp330;
            PlayerEvent.InteractingShootingTarget -= OnInteractingShootingTarget;
            PlayerEvent.InteractedShootingTarget -= OnInteractedShootingTarget;
            PlayerEvent.PlacingBlood -= OnPlacingBlood;
            PlayerEvent.PlacedBlood -= OnPlacedBlood;
            PlayerEvent.PlacingBulletHole -= OnPlacingBulletHole;
            PlayerEvent.PlacedBulletHole -= OnPlacedBulletHole;
            PlayerEvent.SpawningRagdoll -= OnSpawningRagdoll;
            PlayerEvent.SpawnedRagdoll -= OnSpawnedRagdoll;
            PlayerEvent.UnlockingWarheadButton -= OnUnlockingWarheadButton;
            PlayerEvent.UnlockedWarheadButton -= OnUnlockedWarheadButton;
            PlayerEvent.ReceivedAchievement -= OnReceivedAchievement;
            PlayerEvent.RoomChanged -= OnRoomChanged;
            PlayerEvent.ZoneChanged -= OnZoneChanged;
            PlayerEvent.InteractingWarheadLever -= OnInteractingWarheadLever;
            PlayerEvent.InteractedWarheadLever -= OnInteractedWarheadLever;
            PlayerEvent.ChangedSpectator -= OnChangedSpectator;
            PlayerEvent.EnteringHazard -= OnEnteringHazard;
            PlayerEvent.EnteredHazard -= OnEnteredHazard;
            PlayerEvent.StayingInHazard -= OnStayingInHazard;
            PlayerEvent.LeavingHazard -= OnLeavingHazard;
            PlayerEvent.LeftHazard -= OnLeftHazard;
        }

        private static void HandleEvent<T>(T eventArgs) where T : EventArgs
        {
            if (EventTypeMapping.TryGetValue(typeof(T), out ArgumentType argumentType))
            {
                if (eventArgs is IPlayerEvent playerEvent && playerEvent.Player != null)
                {
                    if (playerEvent.Player.CurrentItem is not null && Utilities.TryGetSummonedCustomItem(playerEvent.Player.CurrentItem.Serial, out var item))
                    {
                        if (item.CustomItem.Arguments != null && item.CustomItem.Arguments.Count > 0)
                            ArgumentManager.Trigger(item.CustomItem, argumentType, eventArgs);
                    }
                    else if (playerEvent.Player.Items != null && playerEvent.Player.Items.Count() >= 1)
                    {
                        Item armorItem = playerEvent.Player.Items.Where(i => i.Category is ItemCategory.Armor).FirstOrDefault();
                        if (armorItem is not null && Utilities.TryGetSummonedCustomItem(armorItem.Serial, out var item1))
                        {
                            if (item1.CustomItem.Arguments != null && item1.CustomItem.Arguments.Count > 0)
                                ArgumentManager.Trigger(item1.CustomItem, argumentType, eventArgs);
                        }
                    }
                }
            }
        }

        private static void OnPlayerJoined(PlayerJoinedEventArgs ev) => HandleEvent(ev);
        private static void OnPlayerLeft(PlayerLeftEventArgs ev) => HandleEvent(ev);
        private static void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev) => HandleEvent(ev);
        private static void OnSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev) => HandleEvent(ev);
        private static void OnPreAuthenticating(PlayerPreAuthenticatingEventArgs ev) => HandleEvent(ev);
        private static void OnPreAuthenticated(PlayerPreAuthenticatedEventArgs ev) => HandleEvent(ev);
        private static void OnUsingIntercom(PlayerUsingIntercomEventArgs ev) => HandleEvent(ev);
        private static void OnUsedIntercom(PlayerUsedIntercomEventArgs ev) => HandleEvent(ev);
        private static void OnBanning(PlayerBanningEventArgs ev) => HandleEvent(ev);
        private static void OnBanned(PlayerBannedEventArgs ev) => HandleEvent(ev);
        private static void OnKicking(PlayerKickingEventArgs ev) => HandleEvent(ev);
        private static void OnKicked(PlayerKickedEventArgs ev) => HandleEvent(ev);
        private static void OnMuting(PlayerMutingEventArgs ev) => HandleEvent(ev);
        private static void OnMuted(PlayerMutedEventArgs ev) => HandleEvent(ev);
        private static void OnUnmuting(PlayerUnmutingEventArgs ev) => HandleEvent(ev);
        private static void OnUnmuted(PlayerUnmutedEventArgs ev) => HandleEvent(ev);
        private static void OnReportingCheater(PlayerReportingCheaterEventArgs ev) => HandleEvent(ev);
        private static void OnReportedCheater(PlayerReportedCheaterEventArgs ev) => HandleEvent(ev);
        private static void OnReportingPlayer(PlayerReportingPlayerEventArgs ev) => HandleEvent(ev);
        private static void OnReportedPlayer(PlayerReportedPlayerEventArgs ev) => HandleEvent(ev);
        private static void OnTogglingNoclip(PlayerTogglingNoclipEventArgs ev) => HandleEvent(ev);
        private static void OnToggledNoclip(PlayerToggledNoclipEventArgs ev) => HandleEvent(ev);
        private static void OnJumped(PlayerJumpedEventArgs ev) => HandleEvent(ev);
        private static void OnMovementStateChanged(PlayerMovementStateChangedEventArgs ev) => HandleEvent(ev);
        private static void OnRequestingRaPlayerList(PlayerRequestingRaPlayerListEventArgs ev) => HandleEvent(ev);
        private static void OnRequestedRaPlayerList(PlayerRequestedRaPlayerListEventArgs ev) => HandleEvent(ev);
        private static void OnRaPlayerListAddingPlayer(PlayerRaPlayerListAddingPlayerEventArgs ev) => HandleEvent(ev);
        private static void OnRaPlayerListAddedPlayer(PlayerRaPlayerListAddedPlayerEventArgs ev) => HandleEvent(ev);
        private static void OnRequestedCustomRaInfo(PlayerRequestedCustomRaInfoEventArgs ev) => HandleEvent(ev);
        private static void OnRequestingRaPlayersInfo(PlayerRequestingRaPlayersInfoEventArgs ev) => HandleEvent(ev);
        private static void OnRequestedRaPlayersInfo(PlayerRequestedRaPlayersInfoEventArgs ev) => HandleEvent(ev);
        private static void OnRequestingRaPlayerInfo(PlayerRequestingRaPlayerInfoEventArgs ev) => HandleEvent(ev);
        private static void OnRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev) => HandleEvent(ev);
        private static void OnChangingBadgeVisibility(PlayerChangingBadgeVisibilityEventArgs ev) => HandleEvent(ev);
        private static void OnChangedBadgeVisibility(PlayerChangedBadgeVisibilityEventArgs ev) => HandleEvent(ev);
        private static void OnChangingNickname(PlayerChangingNicknameEventArgs ev) => HandleEvent(ev);
        private static void OnChangedNickname(PlayerChangedNicknameEventArgs ev) => HandleEvent(ev);
        private static void OnGroupChanging(PlayerGroupChangingEventArgs ev) => HandleEvent(ev);
        private static void OnGroupChanged(PlayerGroupChangedEventArgs ev) => HandleEvent(ev);
        private static void OnUpdatingEffect(PlayerEffectUpdatingEventArgs ev) => HandleEvent(ev);
        private static void OnUpdatedEffect(PlayerEffectUpdatedEventArgs ev) => HandleEvent(ev);
        private static void OnDying(PlayerDyingEventArgs ev) => HandleEvent(ev);
        private static void OnDeath(PlayerDeathEventArgs ev) => HandleEvent(ev);
        private static void OnHurting(PlayerHurtingEventArgs ev) => HandleEvent(ev);
        private static void OnHurt(PlayerHurtEventArgs ev) => HandleEvent(ev);
        private static void OnChangingRole(PlayerChangingRoleEventArgs ev) => HandleEvent(ev);
        private static void OnChangedRole(PlayerChangedRoleEventArgs ev) => HandleEvent(ev);
        private static void OnCuffing(PlayerCuffingEventArgs ev) => HandleEvent(ev);
        private static void OnCuffed(PlayerCuffedEventArgs ev) => HandleEvent(ev);
        private static void OnUncuffing(PlayerUncuffingEventArgs ev) => HandleEvent(ev);
        private static void OnUncuffed(PlayerUncuffedEventArgs ev) => HandleEvent(ev);
        private static void OnReceivingLoadout(PlayerReceivingLoadoutEventArgs ev) => HandleEvent(ev);
        private static void OnReceivedLoadout(PlayerReceivedLoadoutEventArgs ev) => HandleEvent(ev);
        private static void OnSpawning(PlayerSpawningEventArgs ev) => HandleEvent(ev);
        private static void OnSpawned(PlayerSpawnedEventArgs ev) => HandleEvent(ev);
        private static void OnChangingItem(PlayerChangingItemEventArgs ev) => HandleEvent(ev);
        private static void OnChangedItem(PlayerChangedItemEventArgs ev) => HandleEvent(ev);
        private static void OnDroppingAmmo(PlayerDroppingAmmoEventArgs ev) => HandleEvent(ev);
        private static void OnDroppedAmmo(PlayerDroppedAmmoEventArgs ev) => HandleEvent(ev);
        private static void OnDroppingItem(PlayerDroppingItemEventArgs ev) => HandleEvent(ev);
        private static void OnDroppedItem(PlayerDroppedItemEventArgs ev) => HandleEvent(ev);
        private static void OnPickingUpAmmo(PlayerPickingUpAmmoEventArgs ev) => HandleEvent(ev);
        private static void OnPickedUpAmmo(PlayerPickedUpAmmoEventArgs ev) => HandleEvent(ev);
        private static void OnPickingUpArmor(PlayerPickingUpArmorEventArgs ev) => HandleEvent(ev);
        private static void OnPickedUpArmor(PlayerPickedUpArmorEventArgs ev) => HandleEvent(ev);
        private static void OnPickingUpItem(PlayerPickingUpItemEventArgs ev) => HandleEvent(ev);
        private static void OnPickedUpItem(PlayerPickedUpItemEventArgs ev) => HandleEvent(ev);
        private static void OnPickingUpScp330(PlayerPickingUpScp330EventArgs ev) => HandleEvent(ev);
        private static void OnPickedUpScp330(PlayerPickedUpScp330EventArgs ev) => HandleEvent(ev);
        private static void OnSearchedAmmo(PlayerSearchedAmmoEventArgs ev) => HandleEvent(ev);
        private static void OnSearchingArmor(PlayerSearchingArmorEventArgs ev) => HandleEvent(ev);
        private static void OnSearchedArmor(PlayerSearchedArmorEventArgs ev) => HandleEvent(ev);
        private static void OnSearchingPickup(PlayerSearchingPickupEventArgs ev) => HandleEvent(ev);
        private static void OnSearchedPickup(PlayerSearchedPickupEventArgs ev) => HandleEvent(ev);
        private static void OnSearchingAmmo(PlayerSearchingAmmoEventArgs ev) => HandleEvent(ev);
        private static void OnThrowingItem(PlayerThrowingItemEventArgs ev) => HandleEvent(ev);
        private static void OnThrewItem(PlayerThrewItemEventArgs ev) => HandleEvent(ev);
        private static void OnThrowingProjectile(PlayerThrowingProjectileEventArgs ev) => HandleEvent(ev);
        private static void OnThrewProjectile(PlayerThrewProjectileEventArgs ev) => HandleEvent(ev);
        private static void OnInspectingKeycard(PlayerInspectingKeycardEventArgs ev) => HandleEvent(ev);
        private static void OnInspectedKeycard(PlayerInspectedKeycardEventArgs ev) => HandleEvent(ev);
        private static void OnSpinningRevolver(PlayerSpinningRevolverEventArgs ev) => HandleEvent(ev);
        private static void OnSpinnedRevolver(PlayerSpinnedRevolverEventArgs ev) => HandleEvent(ev);
        private static void OnToggledDisruptorFiringMode(PlayerToggledDisruptorFiringModeEventArgs ev) => HandleEvent(ev);
        private static void OnUsingItem(PlayerUsingItemEventArgs ev) => HandleEvent(ev);
        private static void OnUsedItem(PlayerUsedItemEventArgs ev) => HandleEvent(ev);
        private static void OnItemUsageEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev) => HandleEvent(ev);
        private static void OnUsingRadio(PlayerUsingRadioEventArgs ev) => HandleEvent(ev);
        private static void OnUsedRadio(PlayerUsedRadioEventArgs ev) => HandleEvent(ev);
        private static void OnAimedWeapon(PlayerAimedWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnDryFiringWeapon(PlayerDryFiringWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnDryFiredWeapon(PlayerDryFiredWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnUnloadingWeapon(PlayerUnloadingWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnUnloadedWeapon(PlayerUnloadedWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnReloadedWeapon(PlayerReloadedWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnShootingWeapon(PlayerShootingWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnShotWeapon(PlayerShotWeaponEventArgs ev) => HandleEvent(ev);
        private static void OnChangingAttachments(PlayerChangingAttachmentsEventArgs ev) => HandleEvent(ev);
        private static void OnChangedAttachments(PlayerChangedAttachmentsEventArgs ev) => HandleEvent(ev);
        private static void OnSendingAttachmentsPrefs(PlayerSendingAttachmentsPrefsEventArgs ev) => HandleEvent(ev);
        private static void OnSentAttachmentsPrefs(PlayerSentAttachmentsPrefsEventArgs ev) => HandleEvent(ev);
        private static void OnCancellingUsingItem(PlayerCancellingUsingItemEventArgs ev) => HandleEvent(ev);
        private static void OnCancelledUsingItem(PlayerCancelledUsingItemEventArgs ev) => HandleEvent(ev);
        private static void OnChangingRadioRange(PlayerChangingRadioRangeEventArgs ev) => HandleEvent(ev);
        private static void OnChangedRadioRange(PlayerChangedRadioRangeEventArgs ev) => HandleEvent(ev);
        private static void OnProcessingJailbirdMessage(PlayerProcessingJailbirdMessageEventArgs ev) => HandleEvent(ev);
        private static void OnProcessedJailbirdMessage(PlayerProcessedJailbirdMessageEventArgs ev) => HandleEvent(ev);
        private static void OnTogglingFlashlight(PlayerTogglingFlashlightEventArgs ev) => HandleEvent(ev);
        private static void OnToggledFlashlight(PlayerToggledFlashlightEventArgs ev) => HandleEvent(ev);
        private static void OnTogglingWeaponFlashlight(PlayerTogglingWeaponFlashlightEventArgs ev) => HandleEvent(ev);
        private static void OnToggledWeaponFlashlight(PlayerToggledWeaponFlashlightEventArgs ev) => HandleEvent(ev);
        private static void OnTogglingRadio(PlayerTogglingRadioEventArgs ev) => HandleEvent(ev);
        private static void OnToggledRadio(PlayerToggledRadioEventArgs ev) => HandleEvent(ev);
        private static void OnDamagingShootingTarget(PlayerDamagingShootingTargetEventArgs ev) => HandleEvent(ev);
        private static void OnDamagedShootingTarget(PlayerDamagedShootingTargetEventArgs ev) => HandleEvent(ev);
        private static void OnDamagingWindow(PlayerDamagingWindowEventArgs ev) => HandleEvent(ev);
        private static void OnDamagedWindow(PlayerDamagedWindowEventArgs ev) => HandleEvent(ev);
        private static void OnEnteringPocketDimension(PlayerEnteringPocketDimensionEventArgs ev) => HandleEvent(ev);
        private static void OnEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev) => HandleEvent(ev);
        private static void OnLeavingPocketDimension(PlayerLeavingPocketDimensionEventArgs ev) => HandleEvent(ev);
        private static void OnLeftPocketDimension(PlayerLeftPocketDimensionEventArgs ev) => HandleEvent(ev);
        private static void OnTriggeringTesla(PlayerTriggeringTeslaEventArgs ev) => HandleEvent(ev);
        private static void OnTriggeredTesla(PlayerTriggeredTeslaEventArgs ev) => HandleEvent(ev);
        private static void OnEscaping(PlayerEscapingEventArgs ev) => HandleEvent(ev);
        private static void OnEscaped(PlayerEscapedEventArgs ev) => HandleEvent(ev);
        private static void OnFlippingCoin(PlayerFlippingCoinEventArgs ev) => HandleEvent(ev);
        private static void OnFlippedCoin(PlayerFlippedCoinEventArgs ev) => HandleEvent(ev);
        private static void OnSearchingToy(PlayerSearchingToyEventArgs ev) => HandleEvent(ev);
        private static void OnSearchedToy(PlayerSearchedToyEventArgs ev) => HandleEvent(ev);
        private static void OnSearchToyAborted(PlayerSearchToyAbortedEventArgs ev) => HandleEvent(ev);
        private static void OnIdlingTesla(PlayerIdlingTeslaEventArgs ev) => HandleEvent(ev);
        private static void OnIdledTesla(PlayerIdledTeslaEventArgs ev) => HandleEvent(ev);
        private static void OnInteractingDoor(PlayerInteractingDoorEventArgs ev) => HandleEvent(ev);
        private static void OnInteractedDoor(PlayerInteractedDoorEventArgs ev) => HandleEvent(ev);
        private static void OnInteractingElevator(PlayerInteractingElevatorEventArgs ev) => HandleEvent(ev);
        private static void OnInteractedElevator(PlayerInteractedElevatorEventArgs ev) => HandleEvent(ev);
        private static void OnInteractingGenerator(PlayerInteractingGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnInteractedGenerator(PlayerInteractedGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnOpeningGenerator(PlayerOpeningGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnOpenedGenerator(PlayerOpenedGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnActivatingGenerator(PlayerActivatingGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnActivatedGenerator(PlayerActivatedGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnDeactivatingGenerator(PlayerDeactivatingGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnDeactivatedGenerator(PlayerDeactivatedGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnUnlockingGenerator(PlayerUnlockingGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnUnlockedGenerator(PlayerUnlockedGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnClosingGenerator(PlayerClosingGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnClosedGenerator(PlayerClosedGeneratorEventArgs ev) => HandleEvent(ev);
        private static void OnInteractingLocker(PlayerInteractingLockerEventArgs ev) => HandleEvent(ev);
        private static void OnInteractedLocker(PlayerInteractedLockerEventArgs ev) => HandleEvent(ev);
        private static void OnInteractingScp330(PlayerInteractingScp330EventArgs ev) => HandleEvent(ev);
        private static void OnInteractedScp330(PlayerInteractedScp330EventArgs ev) => HandleEvent(ev);
        private static void OnInteractingShootingTarget(PlayerInteractingShootingTargetEventArgs ev) => HandleEvent(ev);
        private static void OnInteractedShootingTarget(PlayerInteractedShootingTargetEventArgs ev) => HandleEvent(ev);
        private static void OnPlacingBlood(PlayerPlacingBloodEventArgs ev) => HandleEvent(ev);
        private static void OnPlacedBlood(PlayerPlacedBloodEventArgs ev) => HandleEvent(ev);
        private static void OnPlacingBulletHole(PlayerPlacingBulletHoleEventArgs ev) => HandleEvent(ev);
        private static void OnPlacedBulletHole(PlayerPlacedBulletHoleEventArgs ev) => HandleEvent(ev);
        private static void OnSpawningRagdoll(PlayerSpawningRagdollEventArgs ev) => HandleEvent(ev);
        private static void OnSpawnedRagdoll(PlayerSpawnedRagdollEventArgs ev) => HandleEvent(ev);
        private static void OnUnlockingWarheadButton(PlayerUnlockingWarheadButtonEventArgs ev) => HandleEvent(ev);
        private static void OnUnlockedWarheadButton(PlayerUnlockedWarheadButtonEventArgs ev) => HandleEvent(ev);
        private static void OnReceivedAchievement(PlayerReceivedAchievementEventArgs ev) => HandleEvent(ev);
        private static void OnRoomChanged(PlayerRoomChangedEventArgs ev) => HandleEvent(ev);
        private static void OnZoneChanged(PlayerZoneChangedEventArgs ev) => HandleEvent(ev);
        private static void OnInteractingWarheadLever(PlayerInteractingWarheadLeverEventArgs ev) => HandleEvent(ev);
        private static void OnInteractedWarheadLever(PlayerInteractedWarheadLeverEventArgs ev) => HandleEvent(ev);
        private static void OnChangedSpectator(PlayerChangedSpectatorEventArgs ev) => HandleEvent(ev);
        private static void OnEnteringHazard(PlayerEnteringHazardEventArgs ev) => HandleEvent(ev);
        private static void OnEnteredHazard(PlayerEnteredHazardEventArgs ev) => HandleEvent(ev);
        private static void OnStayingInHazard(PlayersStayingInHazardEventArgs ev) => HandleEvent(ev);
        private static void OnLeavingHazard(PlayerLeavingHazardEventArgs ev) => HandleEvent(ev);
        private static void OnLeftHazard(PlayerLeftHazardEventArgs ev) => HandleEvent(ev);
    } 
}