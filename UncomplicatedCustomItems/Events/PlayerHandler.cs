#if EXILED
using Exiled.CustomRoles.API.Features;
using Exiled.API.Enums;
#endif
using CustomPlayerEffects;
using Footprinting;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Extensions;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents;
using UncomplicatedCustomItems.Events.Handlers;
using UncomplicatedCustomItems.Events.Methods;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;
using UserSettings.ServerSpecific;
using static InventorySystem.Items.Firearms.Modules.AnimatorReloaderModuleBase;
using static InventorySystem.Items.Firearms.Modules.DisruptorActionModule;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using Scp018Projectile = InventorySystem.Items.ThrowableProjectiles.Scp018Projectile;

namespace UncomplicatedCustomItems.Events
{
    internal class PlayerHandler
    {
        internal static Dictionary<Player, CoroutineHandle> _relativePosCoroutine = [];
        internal static Dictionary<Player, CoroutineHandle> _humeShieldRegenCoroutine = [];
        internal static Dictionary<int, CapybaraToy> _capybaras = [];
        internal static Dictionary<Player, long> _damageTimes = [];
        internal static Dictionary<PrimitiveObjectToy, int> _toolGunPrimitives = [];
        public static Dictionary<int, RoleTypeId> Appearance = [];
        internal static readonly CachedLayerMask ToolGunMask = new("Default", "Door", "Glass");
        internal static List<Player> CustomScp268Effects = [];
        internal static List<(CustomItem, ushort, int)> CandyIdx = [];
        private static int AmmoStored = 0;

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public static Dictionary<Pickup, Light> ActiveLights = [];
        /// <summary>
        /// The <see cref="Vector3"/> coordinates of the latest detonation point for a <see cref="ExplosiveGrenadeProjectile"/>.
        /// Triggered by the <see cref="ServerHandler.OnGrenadeExploding"/> method.
        /// </summary>
        public static Vector3 DetonationPosition { get; set; } = Vector3.zero;

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
            PlayerEvent.InteractedDoor += OnDoorInteracted;
            PlayerEvent.InteractingDoor += OnDoorInteracting;
            PlayerEvent.UnlockingGenerator += OnGeneratorUnlock;
            PlayerEvent.InteractingLocker += OnLockerInteracting;
            PlayerEvent.Joined += OnVerified;
            PlayerEvent.PickedUpItem += OnPickup;
            PlayerEvent.PickedUpArmor += OnArmorPickup;
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.Left += OnLeft;
            PlayerEvent.FlippedCoin += OnFlippedCoin;
            PlayerEvent.ToggledFlashlight += OnToggledFlashlight;
            PlayerEvent.ToggledWeaponFlashlight += OnWeaponFlashlightToggled;
            PlayerEvent.ReloadingWeapon += OnReloading;
            PlayerEvent.ReloadedWeapon += OnReloaded;
            PlayerEvent.TogglingFlashlight += OnTogglingFlashlight;
            PlayerEvent.ItemUsageEffectsApplying += OnUsingItemCompleted;
            PlayerEvent.InspectingKeycard += OnInspectingKeycard;
            PlayerEvent.InteractingElevator += OnUsingElevator;
            PlayerEvent.ChangingItem += OnChangingItem;
            PlayerEvent.Death += OnDeath;
            PlayerEvent.ChangingRole += OnRoleChange;
            PlayerEvent.TogglingNoclip += OnNoclip;
            PlayerEvent.ProcessingJailbirdMessage += OnJailbirdMessaging;
            PlayerEvent.ProcessedJailbirdMessage += OnJailbirdMessage;
            PlayerEvent.ThrewProjectile += OnProjectileThrew;
            PlayerEvent.ChangingAttachments += OnPlayerChangingAttachments;
            InventorySystem.InventoryExtensions.OnItemAdded += OnItemAdded;
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
            PlayerEvent.InteractedDoor -= OnDoorInteracted;
            PlayerEvent.InteractingDoor -= OnDoorInteracting;
            PlayerEvent.UnlockingGenerator -= OnGeneratorUnlock;
            PlayerEvent.InteractingLocker -= OnLockerInteracting;
            PlayerEvent.Joined -= OnVerified;
            PlayerEvent.PickedUpItem -= OnPickup;
            PlayerEvent.PickedUpArmor -= OnArmorPickup;
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.Left -= OnLeft;
            PlayerEvent.FlippedCoin -= OnFlippedCoin;
            PlayerEvent.ToggledFlashlight -= OnToggledFlashlight;
            PlayerEvent.ToggledWeaponFlashlight -= OnWeaponFlashlightToggled;
            PlayerEvent.ReloadingWeapon -= OnReloading;
            PlayerEvent.ReloadedWeapon -= OnReloaded;
            PlayerEvent.TogglingFlashlight -= OnTogglingFlashlight;
            PlayerEvent.ItemUsageEffectsApplying -= OnUsingItemCompleted;
            PlayerEvent.InspectingKeycard -= OnInspectingKeycard;
            PlayerEvent.InteractingElevator -= OnUsingElevator;
            PlayerEvent.ChangingItem -= OnChangingItem;
            PlayerEvent.Death -= OnDeath;
            PlayerEvent.ChangingRole -= OnRoleChange;
            PlayerEvent.TogglingNoclip -= OnNoclip;
            PlayerEvent.ProcessingJailbirdMessage -= OnJailbirdMessaging;
            PlayerEvent.ProcessedJailbirdMessage -= OnJailbirdMessage;
            PlayerEvent.ThrewProjectile -= OnProjectileThrew;
            PlayerEvent.ChangingAttachments -= OnPlayerChangingAttachments;
            InventorySystem.InventoryExtensions.OnItemAdded -= OnItemAdded;
        }

        private static void OnItemAdded(ReferenceHub hub, ItemBase itemBase, ItemPickupBase pickupBase)
        {
            Player player = Player.Get(hub);
            if (Utilities.TryGetSummonedCustomItem(itemBase.ItemSerial, out var item))
                item.HandlePickedUpDisplayHint(player);

            if (SummonedAPICustomItem.TryGet(itemBase.ItemSerial, out var api))
                api.HandlePickedUpDisplayHint(player);
        }

        private static void OnPlayerChangingAttachments(PlayerChangingAttachmentsEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out var customItem))
            {
                if (customItem.HasModule(CustomFlags.WorkstationBan))
                {
                    ev.Player.SendHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
                    ev.IsAllowed = false;
                }
            }
        }

        private static void OnProjectileThrew(PlayerThrewProjectileEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.ThrowableItem.Serial, out var summoned))
            {
                summoned.OnThrew(ev);
                switch (summoned.CustomItem.CustomItemType)
                {
                    case CustomItemType.ExplosiveGrenade when summoned.CustomItem.CustomData is ExplosiveGrenadeData exdata && ev.Projectile.Base is ExplosionGrenade exGrenade:
                        exGrenade.MaxRadius = exdata.MaxRadius;
                        exGrenade.ScpDamageMultiplier = exdata.ScpDamageMultiplier;
                        exGrenade._burnedDuration = exdata.BurnDuration;
                        exGrenade._concussedDuration = exdata.ConcussDuration;
                        exGrenade._deafenedDuration = exdata.DeafenDuration;
                        exGrenade._fuseTime = exdata.FuseTime;
                        exGrenade._doorDamageOverDistance.Multiply(exdata.DoorDamageMultiplier);
                        exGrenade._playerDamageOverDistance.Multiply(exdata.PlayerDamageMultiplier);
                        if (exdata.ExplodeOnImpact)
                            exGrenade.gameObject.AddComponent<CollisionHandler>().Init(exGrenade.gameObject, exGrenade);

                        break;
                    
                    case CustomItemType.FlashGrenade when summoned.CustomItem.CustomData is FlashGrenadeData flashdata && ev.Projectile.Base is FlashbangGrenade flash:
                        flash.BlindTime = flashdata.AdditionalBlindedEffect;
                        flash._minimalEffectDuration = flashdata.MinimalDurationEffect;
                        flash._additionalBlurDuration = flashdata.AdditionalBlindedEffect;
                        flash._surfaceZoneDistanceIntensifier = flashdata.SurfaceDistanceIntensifier;
                        flash._fuseTime = flashdata.FuseTime;
                        if (flashdata.ExplodeOnImpact)
                            flash.gameObject.AddComponent<CollisionHandler>().Init(flash.gameObject, flash);

                        break;

                    case CustomItemType.SCPItem when summoned.CustomItem.CustomData is SCP018Data scp018data && ev.Projectile.Base is Scp018Projectile scp018:
                        scp018._friendlyFireTime = scp018data.FriendlyFireTime;
                        scp018._fuseTime = scp018data.FuseTime;
                        break;

                    default:
                        LogManager.Warn($"Unsupported ItemType {ev.ThrowableItem.Type} was thrown as a projectile by {ev.Player.DisplayName}");
                        break;
                }
            }

            if (SummonedAPICustomItem.TryGet(ev.ThrowableItem.Serial, out var api))
            {
                api.OnThrew(ev);
                switch (api.CustomItem)
                {
                    case CustomExplosiveGrenade exdata when ev.Projectile.Base is ExplosionGrenade exGrenade:
                        exGrenade.MaxRadius = exdata.MaxRadius;
                        exGrenade.ScpDamageMultiplier = exdata.ScpDamageMultiplier;
                        exGrenade._burnedDuration = exdata.BurnDuration;
                        exGrenade._concussedDuration = exdata.ConcussDuration;
                        exGrenade._deafenedDuration = exdata.DeafenDuration;
                        exGrenade._fuseTime = exdata.FuseTime;
                        exGrenade._doorDamageOverDistance.Multiply(exdata.DoorDamageMultiplier);
                        exGrenade._playerDamageOverDistance.Multiply(exdata.PlayerDamageMultiplier);
                        if (exdata.ExplodeOnImpact)
                            exGrenade.gameObject.AddComponent<CollisionHandler>().Init(exGrenade.gameObject, exGrenade);

                        break;
                    
                    case CustomFlashGrenade flashdata when ev.Projectile.Base is FlashbangGrenade flash:
                        flash.BlindTime = flashdata.AdditionalBlindedEffect;
                        flash._minimalEffectDuration = flashdata.MinimalDurationEffect;
                        flash._additionalBlurDuration = flashdata.AdditionalBlindedEffect;
                        flash._surfaceZoneDistanceIntensifier = flashdata.SurfaceDistanceIntensifier;
                        flash._fuseTime = flashdata.FuseTime;
                        if (flashdata.ExplodeOnImpact)
                            flash.gameObject.AddComponent<CollisionHandler>().Init(flash.gameObject, flash);

                        break;

                    case CustomSCP018 scp018data when ev.Projectile.Base is Scp018Projectile scp018:
                        scp018._friendlyFireTime = scp018data.FriendlyFireTime;
                        scp018._fuseTime = scp018data.FuseTime;
                        if (scp018data.ExplodeOnImpact)
                            scp018.gameObject.AddComponent<CollisionHandler>().Init(scp018.gameObject, scp018);

                        break;

                    default:
                        LogManager.Warn($"Unsupported ItemType {ev.ThrowableItem.Type} was thrown as a projectile by {ev.Player.DisplayName}");
                        break;
                }
            }
        }

        private static void OnArmorPickup(PlayerPickedUpArmorEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.BodyArmorItem.Serial, out var item))
                return;

            if (item.HasModule(CustomFlags.HumeShield))
            {
                foreach (HumeShieldSettings humeShieldSettings in item.CustomItem.FlagSettings.HumeShieldSettings)
                {
                    ev.Player.MaxHumeShield = humeShieldSettings.MaxHumeShield;
                    ev.Player.HumeShieldRegenCooldown = humeShieldSettings.RegenCoolDown;
                    ev.Player.HumeShieldRegenRate = humeShieldSettings.RegenRate;
                }
            }
        }

        public static void OnJailbirdMessage(PlayerProcessedJailbirdMessageEventArgs ev)
        {
            if (ev.Message is JailbirdMessageType.Inspect)
                ItemInspectionEvents.OnInspectedItem(new InspectedItemEventArgs(ev.JailbirdItem, ev.Player));
        }

        public static void OnJailbirdMessaging(PlayerProcessingJailbirdMessageEventArgs ev)
        {
            if (ev.Message is JailbirdMessageType.Inspect)
            {
                InspectingItemEventArgs args = new(ev.JailbirdItem, ev.Player);
                ItemInspectionEvents.OnInspectingItem(args);
                if (!args.IsAllowed)
                {
                    ev.JailbirdItem.Base.SendRpc(JailbirdMessageType.ChargeFailed);
                    ev.IsAllowed = false;
                }
            }

            if (Utilities.TryGetSummonedCustomItem(ev.JailbirdItem.Serial, out var item) && item.CustomItem.CustomItemType is CustomItemType.Jailbird && item.CustomItem.CustomData is JailbirdData data)
            {
                switch (ev.Message)
                {
                    case JailbirdMessageType.Inspect:
                        item.HandleEvent(ev.Player, ItemEvents.Inspect, ev.JailbirdItem.Serial);
                        break;

                    case JailbirdMessageType.ChargeStarted or JailbirdMessageType.ChargeLoadTriggered:
                        if (item.HasModule(CustomFlags.NoCharge))
                            ev.JailbirdItem.Base.SendRpc(JailbirdMessageType.ChargeFailed);

                        break;
                }
            }
        }

        public static void OnRoleChange(PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!ev.Player.Connection.isReady)
                return;

            if (SummonedAPICustomItem.TryGet(ev.Player.CurrentItem.Serial, out var summonedItem))
            {
                StopHumeShieldRegen(ev.Player);
                summonedItem?.ResetBadge(ev.Player);
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
            StopHumeShieldRegen(ev.Player);
        }

        public static void OnNoclip(PlayerTogglingNoclipEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.HandleEvent(ev.Player, ItemEvents.Noclip, ev.Player.CurrentItem.Serial);
        }

        public static void OnDeath(PlayerDeathEventArgs ev)
        {
            if (!ev.Player.Connection.isReady)
                return;

            if (ev.Player == null)
                return;

            StopHumeShieldRegen(ev.Player);
        }

        internal static IEnumerator<float> DecayRate(Player player, float decayRate)
        {
            for (; ; )
            {
                if (player.HumeShield >= 0)
                {
                    player.HumeShield -= Time.deltaTime * decayRate;
                    yield return Timing.WaitForOneFrame;
                }
                else
                {
                    yield break;
                }
            }
        }

        public static void OnChangingItem(PlayerChangingItemEventArgs ev)
        {
            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem1) && customItem1.CustomItem.CustomItemType is CustomItemType.SCPItem && customItem1.CustomItem.CustomData is SCP268Data data && data.AllowEquipingItems && ev.Player.TryGetEffect(out Invisible invisible) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible.TimeLeft, false));

                if (SummonedAPICustomItem.TryGet(item.Serial, out var customitem2) && customitem2.CustomItem is CustomSCP268 customSCP268 && customSCP268.AllowEquipingItems && ev.Player.TryGetEffect(out Invisible invisible1) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible1.TimeLeft, false));
            }

            if (ev.OldItem is null)
                return;

            if (SummonedAPICustomItem.TryGet(ev.Player.CurrentItem.Serial, out var summonedItem))
            {
                summonedItem?.ResetBadge(ev.Player);

                if (summonedItem.Item.Type == ItemType.GunSCP127 && summonedItem.CustomItem is CustomSCP127 customSCP127)
                {
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(summonedItem.Item.Base);
                    switch (tier)
                    {
                        case Scp127Tier.Tier1:
                            ev.Player.HumeShieldRegenRate = 0f;
                            Timing.RunCoroutine(DecayRate(ev.Player, customSCP127.Tier1ShieldDecayRate));
                            break;
                        case Scp127Tier.Tier2:
                            ev.Player.HumeShieldRegenRate = 0f;
                            Timing.RunCoroutine(DecayRate(ev.Player, customSCP127.Tier2ShieldDecayRate));
                            break;
                        case Scp127Tier.Tier3:
                            ev.Player.HumeShieldRegenRate = 0f;
                            Timing.RunCoroutine(DecayRate(ev.Player, customSCP127.Tier3ShieldDecayRate));
                            break;
                        default:
                            LogManager.Error($"{summonedItem.CustomItem.Name} - {summonedItem.Serial} has no tier or is unsupported tier?");
                            break;
                    }

                    StopHumeShieldRegen(ev.Player);
                }
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
                return;

            customItem?.ResetBadge(ev.Player);

            if (customItem.Item.Type == ItemType.GunSCP127 && customItem.CustomItem.CustomItemType == CustomItemType.SCPItem)
            {
                ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(customItem.Item.Base);
                switch (tier)
                {
                    case Scp127Tier.Tier1:
                        ev.Player.HumeShieldRegenRate = 0f;
                        Timing.RunCoroutine(DecayRate(ev.Player, data.Tier1ShieldDecayRate));
                        break;
                    case Scp127Tier.Tier2:
                        ev.Player.HumeShieldRegenRate = 0f;
                        Timing.RunCoroutine(DecayRate(ev.Player, data.Tier2ShieldDecayRate));
                        break;
                    case Scp127Tier.Tier3:
                        ev.Player.HumeShieldRegenRate = 0f;
                        Timing.RunCoroutine(DecayRate(ev.Player, data.Tier3ShieldDecayRate));
                        break;
                    default:
                        LogManager.Error($"{customItem.CustomItem.Name} - {customItem.Serial} has no tier or is unsupported tier?");
                        break;
                }

                StopHumeShieldRegen(ev.Player);
            }
        }

        public static void OnInspectingKeycard(PlayerInspectingKeycardEventArgs ev)
        {
            if (ev.Player == null || ev.KeycardItem == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.KeycardItem.Serial, out SummonedCustomItem customItem))
                return;

            customItem.HandleEvent(ev.Player, ItemEvents.Inspect, ev.KeycardItem.Serial);
        }

        public static void OnUsingItemCompleted(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (SummonedAPICustomItem.TryGet(ev.UsableItem.Serial, out var summondItem) && summondItem.CustomItem is CustomSCP268 customSCP268)
            {
                Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                {
                    if (!customSCP268.ApplyScp268Effect)
                        ev.Player.DisableEffect<Invisible>();

                    if (customSCP268.ApplyScp268Effect)
                    {
                        ev.Player.EnableEffect<Invisible>(1, customSCP268.Duration, false);
                        CustomScp268Effects.TryAdd(ev.Player);
                    }

                    if (customSCP268.OneTimeUse)
                        ev.UsableItem.DropItem().Destroy();
                });
            }
            if (Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out var customItem))
            {
                if (customItem.CustomItem.CustomItemType is CustomItemType.SCPItem)
                {
                    switch (ev.UsableItem.Type)
                    {
                        case ItemType.SCP268:
                            if (customItem.CustomItem.CustomData is not SCP268Data data)
                                break;

                            Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                            {
                                if (!data.ApplyScp268Effect)
                                    ev.Player.DisableEffect<Invisible>();

                                if (data.ApplyScp268Effect)
                                {
                                    ev.Player.EnableEffect<Invisible>(1, data.Duration, false);
                                    CustomScp268Effects.TryAdd(ev.Player);
                                }

                                if (data.OneTimeUse)
                                    ev.UsableItem.DropItem().Destroy();

#if EXILED
                                if (customItem.HasModule(CustomFlags.Disguise))
                                {
                                    foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                                    {
                                        if (disguiseSettings.RoleId == null)
                                            continue;
                                        if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                                            continue;

                                        Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                                        LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Changing {player.DisplayNickname} appearance to {disguiseSettings.RoleId}");
                                        Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, (RoleTypeId)disguiseSettings.RoleId);
                                        player.Broadcast(10, $"{disguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                                        ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                                        LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Adding or updating {player.Id} to appearance dictionary");
                                        Appearance.TryAdd(player.Id, (RoleTypeId)disguiseSettings.RoleId);
                                    }
                                }
#else
                                if (customItem.HasModule(CustomFlags.Disguise))
                                {
                                    foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                                    {
                                        if (disguiseSettings.RoleId == null)
                                            continue;
                                        if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                                            continue;

                                        LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Changing {ev.Player.Nickname} appearance to {disguiseSettings.RoleId}");
                                        ev.Player.DisguisePlayer((RoleTypeId)disguiseSettings.RoleId);
                                        ev.Player.SendBroadcast($"{disguiseSettings.DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                                        ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                                        LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                                        Appearance.TryAdd(ev.Player.PlayerId, (RoleTypeId)disguiseSettings.RoleId);
                                    }
                                }
#endif
                            });

                            break;
                    }
                }
            }

            if (ev.UsableItem.Base is Scp330Bag bag)
            {
                int idx = bag.SelectedCandyId;
                if (idx < 0 || idx >= bag.Candies.Count)
                    return;

                List<ICustomItem> candies = CustomItem.List.Where(c => c.CustomData is CandyData candyData && c.Spawn.DoSpawn).ToList();
                CustomItem item = candies.RandomItem() as CustomItem;

                if (candies.Count() >= 1)
                {
                    if (CandyIdx.Any(i => i.Item2 == bag.ItemSerial))
                    {
                        if (item.CustomData is CandyData data && bag.Candies[idx] == data.CandyType)
                        {
                            if (data.DestroyOnUse)
                            {
                                bag.TryRemove(idx);
                                CandyIdx.Remove((item, bag.ItemSerial, idx));
                            }

                            if (!data.ApplyEffects)
                            {
                                ev.IsAllowed = false;
                                ev.ContinueProcess = false;
                                InventorySystem.Items.Usables.UsableItemsController.GetHandler(ev.Player.ReferenceHub).CurrentUsable.Item?.OnUsingCancelled();
                                ev.Player.Connection.Send(new InventorySystem.Items.Usables.StatusMessage(InventorySystem.Items.Usables.StatusMessage.StatusType.Cancel, bag.ItemSerial), 0);
                            }

                            ev.Player.SendHint(data.EatingMessage, data.EatingMessageDuration);
                        }
                    }
                    else if (candies.Count > 0 && item.CustomData is CandyData data && UnityEngine.Random.Range(0f, 100f) >= data.Chance && bag.Candies[idx] == data.CandyType)
                    {
                        if (!data.ApplyEffects)
                        {
                            ev.IsAllowed = false;
                            ev.ContinueProcess = false;
                            InventorySystem.Items.Usables.UsableItemsController.GetHandler(ev.Player.ReferenceHub).CurrentUsable.Item?.OnUsingCancelled();
                            ev.Player.Connection.Send(new InventorySystem.Items.Usables.StatusMessage(InventorySystem.Items.Usables.StatusMessage.StatusType.Cancel, bag.ItemSerial), 0);
                        }

                        if (!data.DestroyOnUse)
                            CandyIdx.Add((item, bag.ItemSerial, idx));

                        if (data.DestroyOnUse)
                            bag.TryRemove(idx);

                        ev.Player.SendHint(data.EatingMessage, data.EatingMessageDuration);
                    }
                }

                if (item.HasModule(CustomFlags.DieOnUse))
                {
                    foreach (DieOnUseSettings dieOnUseSettings in item.FlagSettings.DieOnUseSettings)
                    {
                        if (dieOnUseSettings.Vaporize ?? false)
                            ev.Player.Vaporize();

                        if (dieOnUseSettings.DeathMessage != null)
                            ev.Player.Kill($"{dieOnUseSettings.DeathMessage.Replace("%name%", item.Name)}");
                        else
                            ev.Player.Kill($"Killed by {item.Name}");
                    }
                }

                if (item.HasModule(CustomFlags.EffectWhenUsed))
                {
                    foreach (EffectSettings effectSettings in item.FlagSettings.EffectSettings)
                    {
                        if (effectSettings.EffectEvent != null)
                        {
                            if (effectSettings.EffectEvent == "EffectWhenUsed")
                            {
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                                {
                                    LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {item.Id} Name: {item.Name}");
                                    continue;
                                }
                                if (effectSettings.EffectDuration < -1)
                                {
                                    LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {item.Id} Name: {item.Name}");
                                    continue;
                                }
                                if (effectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {item.Id} Name: {item.Name}");
                                    continue;
                                }

                                LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player}");
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
                            LogManager.Error($"{nameof(OnUsingItemCompleted)}: No FlagSettings found on {item.Name}");
                    }
                }

                if (item.HasModule(CustomFlags.CustomSound))
                {
                    LogManager.Debug($"{nameof(OnItemUse)}: Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {item.Name}.");
                    API.Features.AudioSettings settings = item.FlagSettings.AudioSettings.FirstOrDefault();
                    AudioApi.PlayAudio(settings.AudioPath, (float)settings.SoundVolume, ev.Player.Position, (float)settings.AudibleDistance);
                }

                if (item.HasModule(CustomFlags.TantrumOnUse))
                {
                    Vector3 targetPosition = ev.Player.Position;
                    if (Physics.Raycast(ev.Player.Position, Vector3.down, out RaycastHit hitInfo, 3f))
                        targetPosition = hitInfo.point + Vector3.up * 1.25f;

                    TantrumHazard tantrum = TantrumHazard.Spawn(targetPosition, ev.Player.Rotation, new Vector3(1, 1, 1));

                    foreach (TeslaGate gate in TeslaGate.AllGates)
                    {
                        if (gate.IsInIdleRange(ev.Player.Position))
                            gate.TantrumsToBeDestroyed.Add(tantrum.Base);
                    }
                }

                if (item.HasModule(CustomFlags.SwitchRoleOnUse))
                {
                    foreach (SwitchRoleOnUseSettings switchRoleOnUseSettings in item.FlagSettings.SwitchRoleOnUseSettings)
                    {
                        if (switchRoleOnUseSettings.RoleId == null || switchRoleOnUseSettings.RoleType == null || switchRoleOnUseSettings == null)
                            continue;

                        if (switchRoleOnUseSettings.RoleType.ToLower() == "ucr")
                        {
                            if (UCR.TryGetCustomRole((int)switchRoleOnUseSettings.RoleId, out _))
                            {
                                if (switchRoleOnUseSettings.Delay != null || switchRoleOnUseSettings.Delay > 0f)
                                {
                                    Timing.CallDelayed((float)switchRoleOnUseSettings.Delay, () =>
                                    {
                                        UCR.GiveCustomRole((int)switchRoleOnUseSettings.RoleId, ev.Player);
                                    });
                                }
                                else
                                    UCR.GiveCustomRole((int)switchRoleOnUseSettings.RoleId, ev.Player);

                                if (switchRoleOnUseSettings.KeepLocation != null || switchRoleOnUseSettings.KeepLocation != false)
                                {
                                    Vector3 OldPos = ev.Player.Position;
                                    Timing.CallDelayed(0.1f, () =>
                                    {
                                        ev.Player.Position = OldPos;
                                    });
                                }

                                continue;
                            }
                            else
                            {
                                LogManager.Warn($"{switchRoleOnUseSettings.RoleId} Is not a UCR role");
                            }
                        }
#if EXILED
                            else if (switchRoleOnUseSettings.RoleType.ToLower() == "ecr")
                            {
                                if (CustomRole.TryGet((uint)switchRoleOnUseSettings.RoleId, out CustomRole? ECRRole))
                                {
                                    if (switchRoleOnUseSettings.Delay != null || switchRoleOnUseSettings.Delay > 0f)
                                    {
                                        Timing.CallDelayed((float)switchRoleOnUseSettings.Delay, () =>
                                        {
                                            ECRRole.AddRole(Exiled.API.Features.Player.Get(ev.Player));
                                        });
                                    }
                                    else
                                        ECRRole.AddRole(Exiled.API.Features.Player.Get(ev.Player));

                                    if (switchRoleOnUseSettings.KeepLocation != null || switchRoleOnUseSettings.KeepLocation != false)
                                    {
                                        Vector3 OldPos = Exiled.API.Features.Player.Get(ev.Player).Position;
                                        Timing.CallDelayed(0.1f, () =>
                                        {
                                            Exiled.API.Features.Player.Get(ev.Player).Position = OldPos;
                                        });
                                    }
                                    
                                    continue;
                                }
                                else
                                {
                                    LogManager.Warn($"{switchRoleOnUseSettings.RoleId} Is not a ECR role");
                                }
                            }
#endif
                        else if (switchRoleOnUseSettings.RoleType.ToLower() == "normal")
                        {
                            if (ev.Player.Role != (RoleTypeId)switchRoleOnUseSettings.RoleId)
                            {
                                if (switchRoleOnUseSettings.Delay != null || switchRoleOnUseSettings.Delay > 0f)
                                {
                                    Timing.CallDelayed((float)switchRoleOnUseSettings.Delay, () =>
                                    {
                                        ev.Player.SetRole((RoleTypeId)switchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)switchRoleOnUseSettings.SpawnFlags);
                                    });
                                }
                                else
                                    ev.Player.SetRole((RoleTypeId)switchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)switchRoleOnUseSettings.SpawnFlags);

                                continue;
                            }
                        }
#if EXILED
                                else if (switchRoleOnUseSettings.RoleType.ToLower() != "ucr" || switchRoleOnUseSettings.RoleType.ToLower() != "normal" || switchRoleOnUseSettings.RoleType.ToLower() != "ecr")
#else
                        else if (switchRoleOnUseSettings.RoleType.ToLower() != "ucr" || switchRoleOnUseSettings.RoleType.ToLower() != "normal")
#endif
                        {
#if EXILED
                                    LogManager.Warn($"The role_type field in {item.Name} is currently {switchRoleOnUseSettings.RoleType} and should be 'Normal', 'UCR', or 'ECR'");
#else
                            LogManager.Warn($"The role_type field in {item.Name} is currently {switchRoleOnUseSettings.RoleType} and should be 'Normal' or 'UCR'");
#endif
                        }
                    }


                    if (item.HasModule(CustomFlags.Capybara))
                    {
                        CapybaraToy capybara = CapybaraToy.Create(ev.Player.GameObject.transform);
                        capybara.CollidersEnabled = false;
                        capybara.Position += new Vector3(0, -0.8f, 0);
                        ev.Player.Scale = new(0.2f, 0.3f, 0.5f);
                        capybara.Scale = new(6f, 4f, 2.8f);
                        capybara.GameObject.name += "UCI";
                        ev.Player.EnableEffect<Fade>(255, float.MaxValue);
                        _capybaras.TryAdd(ev.Player.PlayerId, capybara);
                    }
#if EXILED
                        if (item.HasModule(CustomFlags.Disguise))
                        {
                            foreach (DisguiseSettings disguiseSettings in item.FlagSettings.DisguiseSettings)
                            {
                                if (disguiseSettings.RoleId == null)
                                    continue;
                                if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                                    continue;

                                Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                                LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Changing {player.DisplayNickname} appearance to {disguiseSettings.RoleId}");
                                Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, (RoleTypeId)disguiseSettings.RoleId);
                                player.Broadcast(10, $"{disguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                                ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                                LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Adding or updating {player.Id} to appearance dictionary");
                                Appearance.TryAdd(player.Id, (RoleTypeId)disguiseSettings.RoleId);
                            }
                        }
#else
                    if (item.HasModule(CustomFlags.Disguise))
                    {
                        foreach (DisguiseSettings disguiseSettings in item.FlagSettings.DisguiseSettings)
                        {
                            if (disguiseSettings.RoleId == null)
                                continue;
                            if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                                continue;

                            LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Changing {ev.Player.Nickname} appearance to {disguiseSettings.RoleId}");
                            ev.Player.DisguisePlayer((RoleTypeId)disguiseSettings.RoleId);
                            ev.Player.SendBroadcast($"{disguiseSettings.DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                            ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                            LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                            Appearance.TryAdd(ev.Player.PlayerId, (RoleTypeId)disguiseSettings.RoleId);
                        }
                    }
#endif
                    if (item.HasModule(CustomFlags.HumeShield))
                    {
                        foreach (HumeShieldSettings humeShieldSettings in item.FlagSettings.HumeShieldSettings)
                        {
                            ev.Player.MaxHumeShield = humeShieldSettings.MaxHumeShield;
                            ev.Player.HumeShieldRegenCooldown = humeShieldSettings.RegenCoolDown;
                            ev.Player.HumeShieldRegenRate = humeShieldSettings.RegenRate;
                        }
                    }
                }
                else
                {
                    LogManager.Info("Selected candy is not tracked as serialized.");
                }
            }
        }

        public static void OnHurt(PlayerHurtEventArgs ev)
        {
            LogManager.Debug($"{ev.Player.Health}/{ev.Player.MaxHealth}");
            foreach (Item pitem in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(pitem.Serial, out var item))
                {
                    if (item.CustomItem.CustomItemType == CustomItemType.Armor && item.HasModule(CustomFlags.Disguise))
                    {
                        foreach (DisguiseSettings disguiseSettings in item.CustomItem.FlagSettings.DisguiseSettings)
                        {
                            if (disguiseSettings.RevealWhenDamaged)
                            {
#if EXILED
                                Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                                Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, player.Role);
#else
                                ev.Player.DisguisePlayer(ev.Player.Role);
#endif
                            }
                        }
                    }
                }
            }

            if (ev.Attacker == null || ev.Attacker.CurrentItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem summonedCustomItem))
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
                    if (effectSettings.EffectEvent != null)
                    {
                        if (effectSettings.EffectEvent == "EffectShot")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effectSettings.Effect))
                            {
                                LogManager.Warn($"Invalid Effect: {effectSettings.Effect} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                continue;
                            }
                            if (effectSettings.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                continue;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                continue;
                            }

                            LogManager.Debug($"Applying effect {effectSettings.Effect} at intensity {effectSettings.EffectIntensity}, duration is {effectSettings.EffectDuration} to {ev.Player.Nickname}");
                            string effect = effectSettings.Effect;
                            float duration = effectSettings.EffectDuration;
                            byte intensity = effectSettings.EffectIntensity;
                            ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, effectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                        LogManager.Error($"No FlagSettings found on {summonedCustomItem.CustomItem.Name}");
                }
            }
        }

        public static void OnTriggeringTesla(PlayerTriggeringTeslaEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || !ev.IsAllowed)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
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
                if (ev.FirearmItem.StoredAmmo >= 0 || ev.FirearmItem.ChamberedAmmo >= 1)
                    ev.IsAllowed = false;
            }
        }

        private static void OnReloaded(PlayerReloadedWeaponEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.HasModule(CustomFlags.SingleFire))
            {
                ev.FirearmItem.StoredAmmo = 0;
                ev.FirearmItem.ChamberedAmmo = 1;
                ev.FirearmItem.Cocked = true;
                ev.FirearmItem.BoltLocked = false;
                if (ev.FirearmItem.ActionModule is AutomaticActionModule actionModule)
                {
                    actionModule._serverQueuedRequests.Clear();
                    actionModule.ServerResync();
                }

                ev.Player.SetAmmo(ev.FirearmItem.AmmoType, ev.Player.Ammo[ev.FirearmItem.AmmoType] -= 1);
            }
        }

        private static void ApplyPhysics(Player player, Pickup pickup, ItemShotSettings settings)
        {
            float num = 1f - Mathf.Abs(Vector3.Dot(player.Camera.forward, Vector3.up));
            Vector3 forward = player.Camera.forward;
            Vector3 vector = player.Camera.up * settings.UpwardsFactor;
            Vector3 vector3 = forward + vector * num;
            Vector3 velocityVector = vector3 * settings.Velocity;

            Rigidbody rb = pickup.PickupStandardPhysics.Rb;
            rb.centerOfMass = Vector3.zero;
            rb.angularVelocity = settings.Torque;
            rb.linearVelocity = velocityVector;

            LogManager.Debug($"Applying physics to {pickup.Type} - {pickup.Serial}: VelocityVector: {velocityVector}, StartTorque: {settings.Torque}, ");
        }

        public static void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.FirearmItem == null)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem1) && customItem1.CustomItem.CustomItemType is CustomItemType.SCPItem && customItem1.CustomItem.CustomData is SCP268Data data && data.AllowShooting && ev.Player.TryGetEffect(out Invisible invisible) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible.TimeLeft, false));

                if (SummonedAPICustomItem.TryGet(item.Serial, out var customitem2) && customitem2.CustomItem is CustomSCP268 customSCP268 && customSCP268.AllowShooting && ev.Player.TryGetEffect(out Invisible invisible1) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible1.TimeLeft, false));
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.HasModule(CustomFlags.ItemShot))
            {
                Vector3 position = ev.Player.Camera.position;
                if (BarrelTipExtension.TryFindWorldmodelBarrelTip(ev.FirearmItem.Serial, out var tip))
                    position = tip.WorldspacePosition;

                position.y -= 0.6f;

                foreach (ItemShotSettings itemShotSettings in customItem.CustomItem.FlagSettings.ItemShotSettings)
                {
                    float num = 1f - Mathf.Abs(Vector3.Dot(ev.Player.Camera.forward, Vector3.up));
                    Vector3 forward = ev.Player.Camera.forward;
                    Vector3 vector = ev.Player.Camera.up * itemShotSettings.UpwardsFactor;
                    Vector3 vector3 = forward + vector * num;
                    Vector3 velocityVector = vector3 * itemShotSettings.Velocity;

                    if (itemShotSettings.IsCustomItem)
                    {
                        SummonedCustomItem summoned = new(Utilities.GetCustomItem(itemShotSettings.CustomItemId), position);
                        ApplyPhysics(ev.Player, summoned.Pickup, itemShotSettings);
                        break;
                    }

                    if ((itemShotSettings.ItemType == ItemType.GrenadeHE || itemShotSettings.ItemType == ItemType.GrenadeFlash || itemShotSettings.ItemType == ItemType.SCP018 || itemShotSettings.ItemType == ItemType.SCP2176) && itemShotSettings.IsGrenade)
                    {
                        int fuse = itemShotSettings.ItemType == ItemType.SCP2176 ? 30 : 10;
                        Pickup spawned = (Pickup)TimedGrenadeProjectile.SpawnActive(position, itemShotSettings.ItemType, ev.Player, fuse);
                        if (spawned != null)
                        {
                            ApplyPhysics(ev.Player, spawned, itemShotSettings);
                            if (itemShotSettings.GrenadeExplodeOnImpact)
                            {
                                if (spawned.Base.Info.ItemId.GetItemBase() is InventorySystem.Items.ThrowableProjectiles.ThrowableItem throwableBase)
                                    spawned.GameObject.AddComponent<CollisionHandler>().Init(spawned.GameObject, throwableBase.Projectile);
                            }

                            LogManager.Debug($"{customItem.CustomItem.Name} - {itemShotSettings.ItemType} spawned (ItemShot) - {spawned.Serial}");
                        }

                        break;
                    }

                    Pickup pickup = Pickup.Create(itemShotSettings.ItemType, position);
                    if (pickup == null)
                    {
                        LogManager.Warn($"{customItem.CustomItem.Name} - Failed to create pickup for ItemType {itemShotSettings.ItemType}");
                        continue;
                    }

                    if (pickup.Base.Info.ItemId.GetItemBase() is InventorySystem.Items.ThrowableProjectiles.ThrowableItem throwableItem)
                    {
                        ThrownProjectile thrownProjectile = UnityEngine.Object.Instantiate(throwableItem.Projectile);
                        if (Pickup.TryGet(thrownProjectile.ItemId.SerialNumber, out var pickup1))
                        {
                            ApplyPhysics(ev.Player, Pickup.Get(thrownProjectile), itemShotSettings);

                            pickup.Base.Info.Locked = true;
                            thrownProjectile.NetworkInfo = pickup.Base.Info;
                            thrownProjectile.PreviousOwner = new Footprint(ev.Player.ReferenceHub);
                            NetworkServer.Spawn(thrownProjectile.gameObject);
                            thrownProjectile.ServerActivate();

                            if (itemShotSettings.GrenadeExplodeOnImpact)
                                pickup.GameObject.AddComponent<CollisionHandler>().Init(pickup.GameObject, throwableItem.Projectile);

                            LogManager.Debug($"{customItem.CustomItem.Name} - ThrownProjectile spawned (ItemShot) - {pickup.Serial}");
                        }
                    }
                    else
                    {
                        ApplyPhysics(ev.Player, pickup, itemShotSettings);
                        pickup.Spawn();

                        LogManager.Debug($"{customItem.CustomItem.Name} - Pickup spawned (ItemShot) - {pickup.Serial}");
                    }
                }
            }

            if (customItem.HasModule(CustomFlags.InfiniteAmmo))
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                customItem.MagazineModule.ServerModifyAmmo(data.MaxMagazineAmmo);
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
                    {
                        LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} vaporized by {customItem.CustomItem.Name} with DieOnUse CustomFlag");
                        ev.Player.Vaporize();
                    }

                    if (dieOnUseSettings.DeathMessage != null)
                    {
                        LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} killed by {customItem.CustomItem.Name} with DieOnUse CustomFlag");
                        ev.Player.Kill($"{dieOnUseSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                    }
                    else
                    {
                        LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} killed by {customItem.CustomItem.Name} with DieOnUse CustomFlag");
                        ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                    }
                }
            }
            if (customItem.HasModule(CustomFlags.DistruptorTracer))
            {
                if (!InventoryItemLoader.TryGetItem(ItemType.ParticleDisruptor, out ParticleDisruptor disruptor))
                    return;
                if (!disruptor.TryGetModule(out ImpactEffectsModule impactmodule))
                    return;
                if (!disruptor.TryGetModule(out DisruptorHitregModule hitregmodule))
                    return;

                Vector3 position1 = ev.Player.Camera.position;
                if (BarrelTipExtension.TryFindWorldmodelBarrelTip(ev.FirearmItem.Serial, out var tip1))
                    position1 = tip1.WorldspacePosition;

                position1.y -= 0.6f;
                float maxDistance = customItem.HitscanHitregModule.DamageFalloffDistance + customItem.HitscanHitregModule.FullDamageDistance;

                Ray baseRay = new(ev.Player.Camera.position + ev.Player.Camera.forward, ev.Player.Camera.forward);

                if (ev.FirearmItem.ActionModule is AutomaticActionModule autoModule)
                {
                    int amount = Mathf.Min(autoModule.AmmoStored, autoModule.ChamberSize);
                    for (int i = 0; i <= amount; i++)
                    {
                        Ray ray = customItem.HitscanHitregModule.RandomizeRay(baseRay, customItem.HitscanHitregModule.CurrentInaccuracy);

                        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, HitscanHitregModuleBase.HitregMask))
                        {
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                        else
                        {
                            Vector3 endPoint = ray.origin + (ray.direction * maxDistance);
                            hitInfo.point = endPoint;
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                    }
                }
                else if (ev.FirearmItem.ActionModule is PumpActionModule pumpModule)
                {
                    for (int i = 0; i <= pumpModule._baseShotsPerTriggerPull; i++)
                    {
                        Ray ray = customItem.HitscanHitregModule.RandomizeRay(baseRay, customItem.HitscanHitregModule.CurrentInaccuracy);

                        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, HitscanHitregModuleBase.HitregMask))
                        {
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                        else
                        {
                            Vector3 endPoint = ray.origin + (ray.direction * maxDistance);
                            hitInfo.point = endPoint;
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                    }
                }
            }
        }

        public static void OnItemUse(PlayerUsedItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.UsableItem == null)
                return;
            if (ev == null)
                return;

            if (SummonedAPICustomItem.TryGet(ev.UsableItem.Serial, out var summonedItem))
                summonedItem?.ResetBadge(ev.Player);

            if (!Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.CustomItem.CustomData is ICandyData)
                return;

            customItem.HandleEvent(ev.Player, ItemEvents.Use, ev.UsableItem.Serial);
            customItem?.ResetBadge(ev.Player);

            if (customItem.CustomItem.Reusable)
                new SummonedCustomItem(customItem.CustomItem, ev.Player);

            if (customItem.HasModule(CustomFlags.HumeShield))
            {
                foreach (HumeShieldSettings humeShieldSettings in customItem.CustomItem.FlagSettings.HumeShieldSettings)
                {
                    ev.Player.MaxHumeShield = humeShieldSettings.MaxHumeShield;
                    ev.Player.HumeShieldRegenCooldown = humeShieldSettings.RegenCoolDown;
                    ev.Player.HumeShieldRegenRate = humeShieldSettings.RegenRate;
                }
            }

#if EXILED
            if (customItem.HasModule(CustomFlags.Disguise))
            {
                foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                {
                    if (disguiseSettings.RoleId == null)
                    continue;
                    if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                    continue;

                    Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                    LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Changing {player.DisplayNickname} appearance to {disguiseSettings.RoleId}");
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, (RoleTypeId)disguiseSettings.RoleId);
                    player.Broadcast(10, $"{disguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                    ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                    LogManager.Debug($"{nameof(OnUsingItemCompleted)}: Adding or updating {player.Id} to appearance dictionary");
                    Appearance.TryAdd(player.Id, (RoleTypeId)disguiseSettings.RoleId);
                }
            }
#else
            if (customItem.HasModule(CustomFlags.Disguise))
            {
                foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                {
                    if (disguiseSettings.RoleId == null)
                        continue;
                    if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                        continue;

                    LogManager.Debug($"{nameof(OnItemUse)}: Changing {ev.Player.Nickname} appearance to {disguiseSettings.RoleId}");
                    ev.Player.DisguisePlayer((RoleTypeId)disguiseSettings.RoleId);
                    ev.Player.SendBroadcast($"{disguiseSettings.DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                    ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                    LogManager.Debug($"{nameof(OnItemUse)}: Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                    Appearance.TryAdd(ev.Player.PlayerId, (RoleTypeId)disguiseSettings.RoleId);
                }
            }
#endif

            if (customItem.HasModule(CustomFlags.TantrumOnUse))
            {
                Vector3 targetPosition = ev.Player.Position;
                if (Physics.Raycast(ev.Player.Position, Vector3.down, out RaycastHit hitInfo, 3f))
                    targetPosition = hitInfo.point + Vector3.up * 1.25f;

                TantrumHazard tantrum = TantrumHazard.Spawn(targetPosition, ev.Player.Rotation, new Vector3(1, 1, 1));

                foreach (TeslaGate gate in TeslaGate.AllGates)
                {
                    if (gate.IsInIdleRange(ev.Player.Position))
                        gate.TantrumsToBeDestroyed.Add(tantrum.Base);
                }
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
                                continue;
                            }
                            if (effectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
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

            if (SummonedAPICustomItem.TryGet(ev.UsableItem.Serial, out var item1))
            {
                switch (ev.UsableItem.Type, item1.CustomItem)
                {
                    case (ItemType.SCP207 or ItemType.AntiSCP207, CustomSCP207 data207):
                        if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == data207.Effect))
                        {
                            LogManager.Warn($"Invalid Effect: {data207.Effect} for ID: {item1.CustomItem.Id} Name: {item1.CustomItem.Name}");
                            return;
                        }
                        if (data207.Duration <= -2)
                        {
                            LogManager.Warn($"Invalid Duration: {data207.Duration} for ID: {item1.CustomItem.Id} Name: {item1.CustomItem.Name}");
                            return;
                        }
                        if (data207.Intensity <= 0)
                        {
                            LogManager.Warn($"Invalid intensity: {data207.Intensity} for ID: {item1.CustomItem.Id} Name: {item1.CustomItem.Name}");
                            return;
                        }
                        LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {data207.Effect} at intensity {data207.Intensity}, duration is {data207.Duration} to {ev.Player.Nickname}");
                        string s207effect = data207.Effect;
                        float s207duration = data207.Duration;
                        byte s207intensity = data207.Intensity;
                        ev.Player?.ReferenceHub.playerEffectsController.ChangeState(s207effect, s207intensity, s207duration, true);
                        break;

                    case (ItemType.SCP1853, CustomSCP1853 data1853):
                        if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == data1853.Effect))
                        {
                            LogManager.Warn($"Invalid Effect: {data1853.Effect} for ID: {item1.CustomItem.Id} Name: {item1.CustomItem.Name}");
                            return;
                        }
                        if (data1853.Duration <= -2)
                        {
                            LogManager.Warn($"Invalid Duration: {data1853.Duration} for ID: {item1.CustomItem.Id} Name: {item1.CustomItem.Name}");
                            return;
                        }
                        if (data1853.Intensity <= 0)
                        {
                            LogManager.Warn($"Invalid intensity: {data1853.Intensity} for ID: {item1.CustomItem.Id} Name: {item1.CustomItem.Name}");
                            return;
                        }
                        LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {data1853.Effect} at intensity {data1853.Intensity}, duration is {data1853.Duration} to {ev.Player.Nickname}");
                        string s1853effect = data1853.Effect;
                        float s1853duration = data1853.Duration;
                        byte s1853intensity = data1853.Intensity;
                        ev.Player?.ReferenceHub.playerEffectsController.ChangeState(s1853effect, s1853intensity, s1853duration, true);
                        break;
                }

                if (ev.UsableItem.Type == ItemType.SCP207 || ev.UsableItem.Type == ItemType.AntiSCP207 && item1.CustomItem is CustomSCP207 customSCP207 && !customSCP207.RemoveItemAfterUse)
                    new SummonedAPICustomItem(item1.CustomItem, ev.Player);

                if (ev.UsableItem.Type == ItemType.SCP1853 && item1.CustomItem is CustomSCP1853 scp1853Data && !scp1853Data.RemoveItemAfterUse)
                    new SummonedAPICustomItem(item1.CustomItem, ev.Player);
            }

            if (Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem item))
            {
                ISCP500Data scp500Data = item.CustomItem.CustomData as ISCP500Data;
                ISCP207Data scp207Data = item.CustomItem.CustomData as ISCP207Data;
                ISCP1853Data scp1853Data = item.CustomItem.CustomData as ISCP1853Data;
                ISCP1576Data scp1576Data = item.CustomItem.CustomData as ISCP1576Data;

                string effect = null;
                byte intensity = 0;
                float duration = 0;

                switch (ev.UsableItem.Type)
                {
                    case ItemType.SCP500 when scp500Data is not null:
                        effect = scp500Data.Effect;
                        intensity = scp500Data.Intensity;
                        duration = scp500Data.Duration;
                        break;

                    case ItemType.SCP207 or ItemType.AntiSCP207 when scp207Data is not null:
                        effect = scp207Data.Effect;
                        intensity = scp207Data.Intensity;
                        duration = scp207Data.Duration;
                        break;

                    case ItemType.SCP1853 when scp1853Data is not null:
                        effect = scp1853Data.Effect;
                        intensity = scp1853Data.Intensity;
                        duration = scp1853Data.Duration;
                        break;

                    case ItemType.SCP1576 when scp1576Data is not null:
                        effect = scp1576Data.Effect;
                        intensity = scp1576Data.Intensity;
                        duration = scp1576Data.Duration;
                        break;

                    default:
                        return;
                }

                if (effect is null)
                    return;

                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == effect))
                {
                    LogManager.Warn($"Invalid Effect: {effect} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                    return;
                }

                if (duration <= -2)
                {
                    LogManager.Warn($"Invalid Duration: {duration} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                    return;
                }

                if (intensity <= 0)
                {
                    LogManager.Warn($"Invalid intensity: {intensity} for ID: {item.CustomItem.Id} Name: {item.CustomItem.Name}");
                    return;
                }

                LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {effect} at intensity {intensity}, duration is {duration} to {ev.Player.Nickname}");
                ev.Player?.ReferenceHub.playerEffectsController.ChangeState(effect, intensity, duration, true);

                if (ev.UsableItem.Type == ItemType.SCP207 || ev.UsableItem.Type == ItemType.AntiSCP207)
                {
                    if (!scp207Data.RemoveItemAfterUse)
                        new SummonedCustomItem(item.CustomItem, ev.Player);
                }

                if (ev.UsableItem.Type == ItemType.SCP1853)
                {
                    if (!scp1853Data.RemoveItemAfterUse)
                        new SummonedCustomItem(item.CustomItem, ev.Player);
                }

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
                if (!Utilities.TryGetSummonedCustomItem(ev.OldItem.Serial, out SummonedCustomItem customItem))
                    return;

                if (customItem.HasModule(CustomFlags.EffectShot) || customItem.HasModule(CustomFlags.EffectWhenEquiped) || customItem.HasModule(CustomFlags.EffectWhenUsed))
                {
                    foreach (EffectSettings effectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                    {
                        foreach (StatusEffectBase effect in ev.Player.ActiveEffects)
                        {
                            if (effect.name == effectSettings.Effect.ToString() && (bool)effectSettings.ClearOnUnequip)
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(effectSettings.Effect, 0, 0);
                        }
                    }
                }
                if (customItem.HasModule(CustomFlags.HumeShield))
                {
                    foreach (HumeShieldSettings humeShieldSettings in customItem.CustomItem.FlagSettings.HumeShieldSettings)
                    {
                        ev.Player.MaxHumeShield -= humeShieldSettings.MaxHumeShield;
                        ev.Player.HumeShieldRegenRate = -1;
                    }
                }
            }

            if (ev.NewItem is not null)
            {
                if (SummonedAPICustomItem.TryGet(ev.NewItem.Serial, out var summonedItem))
                {
                    summonedItem?.LoadBadge(ev.Player);
                    summonedItem.HandleSelectedDisplayHint(ev.Player);

                    if (summonedItem.CustomItem is CustomSCP127 data)
                    {
                        Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(summonedItem.Item.Base);
                        StartHumeShieldRegen(ev.Player, data, tier, summonedItem);
                    }
                }

                if (!Utilities.TryGetSummonedCustomItem(ev.NewItem.Serial, out SummonedCustomItem customItem))
                    return;

                if (customItem.HasModule(CustomFlags.SingleFire) && customItem.MagazineModule.AmmoStored > 1)
                    customItem.MagazineModule.AmmoStored = 1;

                customItem.HandleSelectedDisplayHint(ev.Player);
                customItem?.LoadBadge(ev.Player);

                Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                {
                    if (customItem.CustomItem.CustomItemType is CustomItemType.Light && customItem.Item is LightItem lightSource)
                        lightSource.IsEmitting = false;
                });

                if (customItem.HasModule(CustomFlags.HumeShield))
                {
                    foreach (HumeShieldSettings humeShieldSettings in customItem.CustomItem.FlagSettings.HumeShieldSettings)
                    {
                        ev.Player.MaxHumeShield = humeShieldSettings.MaxHumeShield;
                        ev.Player.HumeShieldRegenCooldown = humeShieldSettings.RegenCoolDown;
                        ev.Player.HumeShieldRegenRate = humeShieldSettings.RegenRate;
                    }
                }

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
                                    continue;
                                }
                                if (effectSettings.EffectDuration < -1)
                                {
                                    LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    continue;
                                }
                                if (effectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    continue;
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
            }
            if (ev.OldItem != null)
            {
                if (SummonedAPICustomItem.TryGet(ev.OldItem.Serial, out var summonedItem) && summonedItem.CustomItem is ToolGun toolGun)
                {
                    SSTwoButtonsSetting clearList = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 23);
                    foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                    {
                        if (_toolGunPrimitives.TryGetValue(primitive, out int id) && clearList.SyncIsA && ev.Player.PlayerId == id)
                            primitive.Destroy();
                    }
                }
            }
        }

        internal static void StartHumeShieldRegen(Player player, CustomSCP127 data, Scp127Tier tier, SummonedAPICustomItem customItem)
        {
            StopHumeShieldRegen(player);
            CoroutineHandle handle = Timing.RunCoroutine(HumeShieldRegeneration(player, data, tier, customItem));
            _humeShieldRegenCoroutine[player] = handle;
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

        internal static IEnumerator<float> HumeShieldRegeneration(Player player, CustomSCP127 data, Scp127Tier tier, SummonedAPICustomItem customItem)
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

            if (SummonedAPICustomItem.TryGet(ev.Item.Serial, out var summonedItem))
            {
                summonedItem.OnPickup(ev);
                summonedItem.HandlePickedUpDisplayHint(ev.Player);
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem))
                return;

            customItem.OnPickup(ev);
            customItem.HandlePickedUpDisplayHint(ev.Player);

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
                                continue;
                            }
                            if (effectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
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
                ev.Player.EnableEffect<Fade>(255, float.MaxValue);
                _capybaras.TryAdd(ev.Player.PlayerId, capybara);
            }
#if EXILED
            if (customItem.HasModule(CustomFlags.Disguise))
            {
                foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                {
                    if (disguiseSettings.RoleId == null)
                        continue;
                    if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                        continue;

                    Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                    LogManager.Debug($"{nameof(OnPickup)}: Changing {player.DisplayNickname} appearance to {disguiseSettings.RoleId}");
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, (RoleTypeId)disguiseSettings.RoleId);
                    player.Broadcast(10, $"{disguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                    ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                    LogManager.Debug($"{nameof(OnPickup)}: Adding or updating {player.Id} to appearance dictionary");
                    Appearance.TryAdd(player.Id, (RoleTypeId)disguiseSettings.RoleId);
                }
            }
#else
            if (customItem.HasModule(CustomFlags.Disguise))
            {
                foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                {
                    if (disguiseSettings.RoleId == null)
                        continue;
                    if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                        continue;

                    LogManager.Debug($"{nameof(OnPickup)}: Changing {ev.Player.Nickname} appearance to {disguiseSettings.RoleId}");
                    ev.Player.DisguisePlayer((RoleTypeId)disguiseSettings.RoleId);
                    ev.Player.SendBroadcast($"{disguiseSettings.DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                    ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                    LogManager.Debug($"{nameof(OnPickup)}: Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                    Appearance.TryAdd(ev.Player.PlayerId, (RoleTypeId)disguiseSettings.RoleId);
                }
            }
#endif
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

            if (SummonedAPICustomItem.TryGet(ev.Attacker.CurrentItem.Serial, out var summonedItem) && summonedItem.CustomItem is CustomWeapon customWeapon)
            {
                if (customWeapon.EnableFriendlyFire)
                {
                    ev.Player.Damage(customWeapon.Damage, ev.Attacker);
                    ev.Attacker.SendHitMarker(customWeapon.Damage);
                }
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out var customItem))
                return;

            if (ev.DamageHandler is JailbirdDamageHandler handler && customItem.CustomItem.CustomItemType is CustomItemType.Jailbird && customItem.CustomItem.CustomData is JailbirdData data)
            {
                if (customItem.Item is LabApi.Features.Wrappers.JailbirdItem item)
                {
                    if (item.IsCharging)
                    {
                        ++data.TotalCharges;
                        data.TotalChargeDamage += handler.Damage;
                    }
                    else
                    {
                        ++data.TotalHits;
                        data.TotalHitDamage += handler.Damage;
                    }

                    data.TotalDamage += handler.Damage;
                }
            }

            if (customItem.CustomItem.CustomItemType is CustomItemType.Weapon)
            {
                IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;
                if (weaponData.EnableFriendlyFire)
                {
                    ev.Player.Damage(weaponData.Damage, ev.Attacker);
                    ev.Attacker.SendHitMarker(weaponData.Damage);
                }
            }

            switch (customItem.CustomItem.CustomItemType)
            {
                case CustomItemType.Weapon:
                {
                    IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;
                    FirearmDamageHandler damageHandler = ev.DamageHandler as FirearmDamageHandler;
                    LogManager.Debug($"{damageHandler.Damage}");
                    LogManager.Debug($"{ev.Player.Health}/{ev.Player.MaxHealth}");
                    damageHandler.Damage = weaponData.Damage;
                    LogManager.Debug($"{damageHandler.Damage}");
                    break;
                }

                case CustomItemType.MicroHID:
                {
                    IMicroHIDData microData = customItem.CustomItem.CustomData as IMicroHIDData;
                    MicroHidDamageHandler damageHandler = ev.DamageHandler as MicroHidDamageHandler;
                    damageHandler.Damage = microData.Damage;
                    break;
                }

                case CustomItemType.ParticleDisruptor:
                {
                    IParticleDisruptorData disruptorData = customItem.CustomItem.CustomData as IParticleDisruptorData;
                    DisruptorDamageHandler damageHandler = ev.DamageHandler as DisruptorDamageHandler;
                    if (damageHandler.FiringState == FiringState.FiringSingle)
                        damageHandler.Damage = disruptorData.ChargeDamage;
                        
                    if (damageHandler.FiringState == FiringState.FiringRapid)
                        damageHandler.Damage = disruptorData.BurstDamage;

                    break;
                }
            }
        }

        public static void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.Role == RoleTypeId.Spectator || ev.Player.Role == RoleTypeId.Destroyed)
                return;

            CapybaraToy[] capybaras = [];
            capybaras = ev.Player.GameObject.GetComponentsInChildren<CapybaraToy>();
            foreach (CapybaraToy toy in capybaras)
            {
                toy.Parent = Player.Host.GameObject.transform;
                toy.Position = new(1000, 1000, 1000);
                toy.Scale = new(0, 0, 0);
                toy.Destroy();
                ev.Player.DisableEffect<Fade>();

                ev.Player.Scale = new(1f, 1f, 1f);
            }

            Timing.CallDelayed(0.1f, () =>
            {
                foreach (Item item in ev.Player.Items)
                {
                    if (!Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem))
                        return;

                    if (customItem.HasModule(CustomFlags.HumeShield))
                    {
                        foreach (HumeShieldSettings humeShieldSettings in customItem.CustomItem.FlagSettings.HumeShieldSettings)
                        {
                            ev.Player.MaxHumeShield = humeShieldSettings.MaxHumeShield;
                            ev.Player.HumeShieldRegenCooldown = humeShieldSettings.RegenCoolDown;
                            ev.Player.HumeShieldRegenRate = humeShieldSettings.RegenRate;
                        }
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
                                        continue;
                                    }
                                    if (effectSettings.EffectDuration < -1)
                                    {
                                        LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                        continue;
                                    }
                                    if (effectSettings.EffectIntensity <= 0)
                                    {
                                        LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                        continue;
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
#if EXILED
                    if (customItem.HasModule(CustomFlags.Disguise))
                    {
                        foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                        {
                            if (disguiseSettings.RoleId == null)
                                continue;
                            if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                                continue;
                                
                            Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                            LogManager.Debug($"{nameof(OnSpawned)}: Changing {player.DisplayNickname} appearance to {disguiseSettings.RoleId}");
                            Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, (RoleTypeId)disguiseSettings.RoleId);
                            player.Broadcast(10, $"{disguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                            ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                            LogManager.Debug($"{nameof(OnSpawned)}: Adding or updating {player.Id} to appearance dictionary");
                            Appearance.TryAdd(player.Id, (RoleTypeId)disguiseSettings.RoleId);
                        }
                    }
#else
                    if (customItem.HasModule(CustomFlags.Disguise))
                    {
                        foreach (DisguiseSettings disguiseSettings in customItem.CustomItem.FlagSettings.DisguiseSettings)
                        {
                            if (disguiseSettings.RoleId == null)
                                continue;
                            if (string.IsNullOrWhiteSpace(disguiseSettings.DisguiseMessage))
                                continue;

                            LogManager.Debug($"{nameof(OnSpawned)}: Changing {ev.Player.Nickname} appearance to {disguiseSettings.RoleId}");
                            ev.Player.DisguisePlayer((RoleTypeId)disguiseSettings.RoleId);
                            ev.Player.SendBroadcast($"{disguiseSettings.DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                            ev.Player.CustomInfo = disguiseSettings.CustomInfo;
                            LogManager.Debug($"{nameof(OnSpawned)}: Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                            Appearance.TryAdd(ev.Player.PlayerId, (RoleTypeId)disguiseSettings.RoleId);
                        }
                    }
#endif
                }
            });
        }

        public static void OnUsingElevator(PlayerInteractingElevatorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.IsAllowed == false)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem1) && customItem1.CustomItem.CustomItemType is CustomItemType.SCPItem && customItem1.CustomItem.CustomData is SCP268Data data && data.AllowUsingElevators && ev.Player.TryGetEffect(out Invisible invisible) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible.TimeLeft, false));

                if (SummonedAPICustomItem.TryGet(item.Serial, out var customitem2) && customitem2.CustomItem is CustomSCP268 customSCP268 && customSCP268.AllowUsingElevators && ev.Player.TryGetEffect(out Invisible invisible1) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible1.TimeLeft, false));
            }
        }

        public static void OnDoorInteracting(PlayerInteractingDoorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.IsAllowed == false)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem1) && customItem1.CustomItem.CustomItemType is CustomItemType.SCPItem && customItem1.CustomItem.CustomData is SCP268Data data && data.AllowOpeningDoors && ev.Player.TryGetEffect(out Invisible invisible) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible.TimeLeft, false));

                if (SummonedAPICustomItem.TryGet(item.Serial, out var customitem2) && customitem2.CustomItem is CustomSCP268 customSCP268 && customSCP268.AllowOpeningDoors && ev.Player.TryGetEffect(out Invisible invisible1) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible1.TimeLeft, false));
            }
        }

        public static void OnDoorInteracted(PlayerInteractedDoorEventArgs ev)
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
                switch (customItem.CustomItem.CustomItemType)
                {
                    case CustomItemType.Keycard:
                        IKeycardData data = customItem.CustomItem.CustomData as IKeycardData;
                        if (ev.Door.Base.IsMoving && data.OneTimeUse)
                        {
                            Timing.CallDelayed(0.5f, () =>
                            {
                                ev.Player.SendHint($"{data.OneTimeUseHint.Replace("%name%", customItem.CustomItem.Name)}", 8f);
                                LogManager.Debug($"OneTimeUse is true removing {customItem.CustomItem.Name}...");
                                ev.Player.RemoveItem(customItem.Item);
                                customItem?.ResetBadge(ev.Player);
                            });
                        }

                        break;
                }
            }
        }

        public static void OnGeneratorUnlock(PlayerUnlockingGeneratorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.IsAllowed == false)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem1) && customItem1.CustomItem.CustomItemType is CustomItemType.SCPItem && customItem1.CustomItem.CustomData is SCP268Data data && data.AllowOpeningGenerators && ev.Player.TryGetEffect(out Invisible invisible) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible.TimeLeft, false));

                if (SummonedAPICustomItem.TryGet(item.Serial, out var customitem2) && customitem2.CustomItem is CustomSCP268 customSCP268 && customSCP268.AllowOpeningGenerators && ev.Player.TryGetEffect(out Invisible invisible1) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible1.TimeLeft, false));
            }

            if (ev.Player.CurrentItem == null)
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
            if (ev.IsAllowed == false)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem1) && customItem1.CustomItem.CustomItemType is CustomItemType.SCPItem && customItem1.CustomItem.CustomData is SCP268Data data && data.AllowEquipingItems && ev.Player.TryGetEffect(out Invisible invisible) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible.TimeLeft, false));

                if (SummonedAPICustomItem.TryGet(item.Serial, out var customitem2) && customitem2.CustomItem is CustomSCP268 customSCP268 && customSCP268.AllowEquipingItems && ev.Player.TryGetEffect(out Invisible invisible1) && CustomScp268Effects.Contains(ev.Player))
                    Timing.CallDelayed(Timing.WaitForOneFrame, () => ev.Player.EnableEffect<Invisible>(1, invisible1.TimeLeft, false));
            }

            if (ev.Player.CurrentItem == null)
                return;

            if (SummonedAPICustomItem.TryGet(ev.Player.CurrentItem.Serial, out var summonedItem) && summonedItem.CustomItem is CustomKeycard keycard)
            {
                if (ev.Chamber.IsOpen && keycard.OneTimeUse)
                {
                    Timing.CallDelayed(0.5f, () =>
                    {
                        ev.Player.SendHint($"{keycard.OneTimeUseMessage.Replace("%name%", summonedItem.CustomItem.Name)}", keycard.OneTimeUseMessageDuration);
                        LogManager.Debug($"OneTimeUse is true removing {summonedItem.CustomItem.Name}...");
                        ev.Player.RemoveItem(summonedItem.Item);
                    });
                }
            }

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

            if (SummonedAPICustomItem.TryGet(ev.Player.CurrentItem.Serial, out var summonedItem))
                summonedItem?.ResetBadge(ev.Player);

            if (!Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem customItem))
                return;

            customItem?.ResetBadge(ev.Player);

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
                                continue;
                            }
                            if (effectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
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
                        LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                }
            }
        }

        public static void OnDrop(PlayerDroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;

            if (SummonedAPICustomItem.TryGet(ev.Pickup.Serial, out var summonedItem))
            {
                NetworkServer.UnSpawn(ev.Pickup.GameObject);
                ev.Pickup.GameObject.transform.localScale = summonedItem.CustomItem.Scale;
                ev.Pickup.Weight = summonedItem.CustomItem.Weight;
                ev.Pickup.Spawn();

                if (summonedItem.CustomItem is ToolGun toolGun)
                    summonedItem.Destroy();

                summonedItem.OnDrop(ev);
                summonedItem?.ResetBadge(ev.Player);
                StopHumeShieldRegen(ev.Player);
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem summonedCustomItem))
                return;

            summonedCustomItem.OnDrop(ev);
            summonedCustomItem?.ResetBadge(ev.Player);
            StopHumeShieldRegen(ev.Player);

            try
            {
                NetworkServer.UnSpawn(ev.Pickup.GameObject);
                ev.Pickup.GameObject.transform.localScale = summonedCustomItem.CustomItem.Scale;
                ev.Pickup.Weight = summonedCustomItem.CustomItem.Weight;
                ev.Pickup.Spawn();
            }
            catch (Exception ex)
            {
                LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id} - {summonedCustomItem.CustomItem.CustomFlags}");
                LogManager.Error($"Couldnt set CustomItem Pickup Scale or CustomItem Pickup Weight\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
            }

            if (summonedCustomItem.HasModule(CustomFlags.ItemGlow))
            {
                foreach (ItemGlowSettings itemGlowSettings in summonedCustomItem.CustomItem.FlagSettings.ItemGlowSettings)
                {
                    if (ev.Pickup.Base.gameObject == null)
                        return;

                    Color lightColor = Color.blue;

                    if (itemGlowSettings != null)
                    {
                        if (!string.IsNullOrEmpty(itemGlowSettings.GlowColor))
                        {
                            if (ColorUtility.TryParseHtmlString(itemGlowSettings.GlowColor, out Color parsedColor))
                            {
                                lightColor = parsedColor;
                            }
                            else
                            {
                                LogManager.Error($"Failed to parse color: {itemGlowSettings.GlowColor} for {summonedCustomItem.CustomItem.Name}");
                            }
                        }
                    }
                    else
                        LogManager.Error("No FlagSettings found on custom item");

                    Light light = Light.Create(ev.Pickup.Position);
                    light.Color = lightColor;
                    light.Intensity = itemGlowSettings.Intensity;
                    light.Range = itemGlowSettings.Range;
                    light.ShadowType = LightShadows.None;

                    light.Base.gameObject.transform.SetParent(ev.Pickup.Base.gameObject.transform, true);
                    light.Position += Vector3.up * 0.3f;
                    LogManager.Debug($"Item Light spawned at position: {light.Position}");
                    ActiveLights[ev.Pickup] = light;
                }
            }

            if (summonedCustomItem.HasModule(CustomFlags.HumeShield) && ev.Pickup.Type is ItemType.ArmorLight || ev.Pickup.Type is ItemType.ArmorCombat || ev.Pickup.Type is ItemType.ArmorHeavy)
            {
                ev.Player.MaxHumeShield = 0;
                ev.Player.HumeShieldRegenCooldown = 0;
                ev.Player.HumeShieldRegenRate = 0;
            }

#if EXILED
            if (summonedCustomItem.HasModule(CustomFlags.Disguise))
            {
                Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Player);
                Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, ev.Player.Role);
            }
#else
            if (summonedCustomItem.HasModule(CustomFlags.Disguise))
                ev.Player.DisguisePlayer(ev.Player.Role);
#endif

            if (summonedCustomItem.HasModule(CustomFlags.Capybara))
            {
                CapybaraToy[] capybaras = [];
                capybaras = ev.Player.GameObject.GetComponentsInChildren<CapybaraToy>();
                foreach (CapybaraToy toy in capybaras)
                {
                    toy.Parent = Player.Host.GameObject.transform;
                    toy.Position = new(1000, 1000, 1000);
                    toy.Destroy();
                    ev.Player.DisableEffect<Fade>();
                    ev.Player.Scale = new(1f, 1f, 1f);
                }
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
            if (summonedCustomItem.HasModule(CustomFlags.DieOnDrop))
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
        }

        public static void OnDropping(PlayerDroppingItemEventArgs ev)
        {
            if (ev.Item is null || ev.Player == null)
                return;

            if (ev.Item.Base is Scp330Bag bag)
            {
                List<ICustomItem> candies = CustomItem.List.Where(c => c.CustomData is CandyData candyData).ToList();
                CustomItem item = candies.RandomItem() as CustomItem;

                if (candies.Count > 0 && item.CustomData is CandyData data && bag.Candies[bag.SelectedCandyId] == data.CandyType)
                {
                    if (item.HasModule(CustomFlags.CantDrop))
                    {
                        ev.IsAllowed = false;
                        foreach (CantDropSettings cantDropSettings in item.FlagSettings.CantDropSettings)
                        {
                            if (!string.IsNullOrWhiteSpace(cantDropSettings.HintOrBroadcast) && cantDropSettings.HintOrBroadcast.ToLower() == "hint")
                            {
                                if (!string.IsNullOrWhiteSpace(cantDropSettings.Message) && cantDropSettings.Duration != null && cantDropSettings.Duration >= 1)
                                {
                                    try
                                    {
                                        LogManager.Silent("Name | Id | CustomFlag(s)");
                                        LogManager.Silent($"{item.Name} - {item.Id} - {item.CustomFlags}");
                                        LogManager.Debug($"Sending CantDrop Hint to {ev.Player.Nickname}\nHint: {cantDropSettings.Message.Replace("%name%", item.Name)}");
                                        ev.Player.SendHint($"{cantDropSettings.Message.Replace("%name%", item.Name)}", (ushort)cantDropSettings.Duration);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Silent("Name | Id | CustomFlag(s)");
                                        LogManager.Silent($"{item.Name} - {item.Id} - {item.CustomFlags}");
                                        LogManager.Error($"Couldnt send CantDrop Hint to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                                    }
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(cantDropSettings.HintOrBroadcast) && cantDropSettings.HintOrBroadcast.ToLower() == "broadcast")
                            {
                                if (!string.IsNullOrWhiteSpace(cantDropSettings.Message) && cantDropSettings.Duration != null && cantDropSettings.Duration >= 1)
                                {
                                    try
                                    {
                                        LogManager.Silent("Name | Id | CustomFlag(s)");
                                        LogManager.Silent($"{item.Name} - {item.Id} - {item.CustomFlags}");
                                        LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.Nickname}\nBroadcast: {cantDropSettings.Message.Replace("%name%", item.Name)}");
                                        ev.Player.SendBroadcast($"{cantDropSettings.Message.Replace("%name%", item.Name)}", (ushort)cantDropSettings.Duration, Broadcast.BroadcastFlags.Normal, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Silent("Name | Id | CustomFlag(s)");
                                        LogManager.Silent($"{item.Name}  -  {item.Id}  -  {item.CustomFlags}");
                                        LogManager.Error($"Couldnt send CantDrop Broadcast to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                                    }
                                }
                            }
                            else
                                LogManager.Warn($"CantDropSettings HintOrBroadcast for {item.Name} is {cantDropSettings.HintOrBroadcast} Expected values are 'hint' or 'broadcast'");
                        }
                    }
                }
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.HasModule(CustomFlags.CantDrop))
            {
                ev.IsAllowed = false;
                foreach (CantDropSettings cantDropSettings in customItem.CustomItem.FlagSettings.CantDropSettings)
                {
                    if (!string.IsNullOrWhiteSpace(cantDropSettings.HintOrBroadcast) && cantDropSettings.HintOrBroadcast.ToLower() == "hint")
                    {
                        if (!string.IsNullOrWhiteSpace(cantDropSettings.Message) && cantDropSettings.Duration != null && cantDropSettings.Duration >= 1)
                        {
                            try
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Debug($"Sending CantDrop Hint to {ev.Player.Nickname}\nHint: {cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}");
                                ev.Player.SendHint($"{cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}", (ushort)cantDropSettings.Duration);
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Hint to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                            }
                        }
                    }
                    else if (cantDropSettings.HintOrBroadcast != null && cantDropSettings.HintOrBroadcast.ToLower() == "broadcast")
                    {
                        if (!string.IsNullOrWhiteSpace(cantDropSettings.Message) && cantDropSettings.Duration != null && cantDropSettings.Duration >= 1)
                        {
                            try
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                                LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.Nickname}\nBroadcast: {cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}");
                                ev.Player.SendBroadcast($"{cantDropSettings.Message.Replace("%name%", customItem.CustomItem.Name)}", (ushort)cantDropSettings.Duration, Broadcast.BroadcastFlags.Normal, true);
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
        }

        public static void OnDying(PlayerDyingEventArgs ev)
        {
            CapybaraToy[] capybaras = [];
            capybaras = ev.Player.GameObject.GetComponentsInChildren<CapybaraToy>();
            foreach (CapybaraToy toy in capybaras)
            {
                toy.Parent = Player.Host.GameObject.transform;
                toy.Position = new(1000, 1000, 1000);
                toy.Scale = new(0, 0, 0);
                toy.Destroy();
                ev.Player.DisableEffect<Fade>();
                ev.Player.Scale = new(1f, 1f, 1f);
            }

            CustomScp268Effects.Remove(ev.Player);

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
            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem customItem))
                return;

            PlayerExtensions.PlayerKills.TryGetValue(ev.Attacker, out int kills);
            PlayerExtensions.PlayerKills.TryAdd(ev.Attacker, kills + 1);

            if (customItem.HasModule(CustomFlags.ChangeDisguiseOnKill))
            {
#if EXILED
                Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ev.Attacker);
                LogManager.Debug($"{nameof(OnDying)}: Changing {player.DisplayNickname} appearance to {ev.Player.Role}");
                Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, ev.Player.Role);
                LogManager.Debug($"{nameof(OnDying)}: Adding or updating {player.Id} to appearance dictionary");
                Appearance.TryAdd(player.Id, ev.Player.Role);
#else
                LogManager.Debug($"{nameof(OnDying)}: Changing {ev.Player.Nickname} appearance to {ev.Player.Role}");
                ev.Attacker.DisguisePlayer(ev.Player.Role);
                LogManager.Debug($"{nameof(OnDying)}: Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                Appearance.TryAdd(ev.Player.PlayerId, ev.Player.Role);
#endif
            }

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

        public static void OnVerified(PlayerJoinedEventArgs ev)
        {
            if (BadgeManager.devBadges.ContainsKey(ev.Player.UserId) && Plugin.Instance.Config.AllowDevPermissions)
            {
                LogManager.Debug($"Applying developer usergroup to {ev.Player.DisplayName} - {ev.Player.PlayerId} - {ev.Player.UserId}");
                LogManager.Security($"Allow Dev Permissions is enabled in your config! Any UCI developers can run commands on your server. If this was not intended, please disable it.");
                var (badgeText, badgeColor) = BadgeManager.devBadges[ev.Player.UserId];
                UserGroup userGroup = new()
                {
                    Permissions = ulong.MaxValue,
                    BadgeColor = badgeColor,
                    BadgeText = badgeText
                };

                ev.Player.UserGroup = userGroup;
                LogManager.Debug($"Developer UserGroup applied to {ev.Player.DisplayName} - {ev.Player.PlayerId} - {ev.Player.UserId}");
            }

            foreach (KeyValuePair<int, RoleTypeId> entry in Appearance)
            {
                LogManager.Debug($"{nameof(OnVerified)}: Changing {entry.Key} appearance to {entry.Value}");
#if EXILED
                Exiled.API.Features.Player.TryGet(entry.Key, out Exiled.API.Features.Player player);
                Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, entry.Value);
#else
                Player.TryGet(entry.Key, out Player player);
                player.DisguisePlayer(entry.Value);
#endif
            }
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.IsHost)
                return;

            CustomScp268Effects.Remove(ev.Player);
#if EXILED
            if (Appearance.ContainsKey(ev.Player.PlayerId))
            {
                LogManager.Debug($"{nameof(OnLeft)}: Removing {ev.Player.PlayerId} from appearance dictionary");
                Appearance.TryRemove(ev.Player.PlayerId);
            }
#endif
            if (_capybaras.ContainsKey(ev.Player.PlayerId))
                _capybaras.TryRemove(ev.Player.PlayerId);
        }

        public static void OnShot(PlayerShotWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.HasModule(CustomFlags.SingleFire))
                customItem.MagazineModule.AmmoStored = 0;

            if (customItem.HasModule(CustomFlags.AmmoRegen))
            {
                AmmoRegenSettings regen = customItem.CustomItem.FlagSettings.AmmoRegenSettings.FirstOrDefault();
                customItem.PauseAmmoRegen(ev.FirearmItem, regen.RegenDelay);
            }

            if (ev.FirearmItem.ActionModule is AutomaticActionModule actionModule)
            {
                actionModule._serverQueuedRequests.Clear();
                actionModule._clientQueuedShots.Clear();
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
                                continue;
                            }
                            if (effectSettings.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {effectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
                            }
                            if (effectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {effectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                continue;
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
            if (Physics.Raycast(ev.Player.Camera.position + ev.Player.Camera.forward, ev.Player.Camera.forward, out RaycastHit hitInfo, customItem.HitscanHitregModule.DamageFalloffDistance + customItem.HitscanHitregModule.FullDamageDistance, ToolGunMask))
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
            }
        }

        public static void OnReceivingEffect(PlayerEffectUpdatingEventArgs ev)
        {
            if (ev.Effect == null)
                return;
            if (ev.Player == null)
                return;

            if (CustomScp268Effects.Contains(ev.Player) && ev.Effect is Invisible invisible && invisible.TimeLeft < 1)
            {
                ev.Player.DisableEffect(ev.Effect);
                CustomScp268Effects.Remove(ev.Player);
            }

            if (ev.Player.CurrentItem == null)
                return;

            if (SummonedAPICustomItem.TryGet(ev.Player.CurrentItem.Serial, out var summonedItem))
            {
                switch (ev.Effect, summonedItem.CustomItem)
                {
                    case (Scp207 or AntiScp207, CustomSCP207 scp207Data):
                        LogManager.Debug("Effect is from a 207 custom item.");
                        if (!scp207Data.Apply207Effect)
                        {
                            LogManager.Debug("Removing SCP-207 effect.");
                            ev.Player.DisableEffect(ev.Effect);
                            ev.IsAllowed = false;
                        }
                        break;

                    case (Scp1853, CustomSCP1853 scp1853Data):
                        LogManager.Debug("Effect is from a 1853 custom item.");
                        if (!scp1853Data.Apply1853Effect)
                        {
                            LogManager.Debug("Removing SCP-1853 effect.");
                            ev.Player.DisableEffect(ev.Effect);
                            ev.IsAllowed = false;
                        }
                        break;
                }
            }

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem))
            {
                LogManager.Debug($"{ev.Player.Nickname} is receiving {ev.Effect}");
                switch (ev.Effect)
                {
                    case AntiScp207 or Scp207 when customItem.CustomItem.CustomData is SCP207Data scp207Data:
                        LogManager.Debug("Effect is from a 207 custom item.");
                        if (!scp207Data.Apply207Effect)
                        {
                            LogManager.Debug("Removing SCP-207 effect.");
                            ev.Player.DisableEffect(ev.Effect);
                            ev.IsAllowed = false;
                        }
                        break;
                    case Scp1853 when customItem.CustomItem.CustomData is SCP1853Data scp1853Data:
                        LogManager.Debug("Effect is from a 1853 custom item.");
                        if (!scp1853Data.Apply1853Effect)
                        {
                            LogManager.Debug("Removing SCP-1853 effect.");
                            ev.Player.DisableEffect(ev.Effect);
                            ev.IsAllowed = false;
                        }
                        break;
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
        }
    }
}