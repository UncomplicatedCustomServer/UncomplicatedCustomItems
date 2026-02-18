#if EXILED
using Exiled.CustomRoles.API.Features;
using Exiled.API.Enums;
#endif
using CustomPlayerEffects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.Pickups;
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
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules;
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
using UncomplicatedCustomItems.Integrations;
using UnityEngine;
using UserSettings.ServerSpecific;
using static InventorySystem.Items.Firearms.Modules.DisruptorActionModule;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using InventorySystem.Items.Autosync;

namespace UncomplicatedCustomItems.Events
{
    internal class PlayerHandler
    {
        internal static Dictionary<Player, CoroutineHandle> _humeShieldRegenCoroutine = [];
        internal static Dictionary<int, CapybaraToy> _capybaras = [];
        internal static Dictionary<Player, long> _damageTimes = [];
        internal static Dictionary<PrimitiveObjectToy, int> _toolGunPrimitives = [];
        public static Dictionary<int, RoleTypeId> Appearance = [];
        internal static readonly CachedLayerMask ToolGunMask = new("Default", "Door", "Glass");
        internal static List<Player> CustomScp268Effects = [];
        internal static List<(CustomItem, ushort, int)> CandyIdx = [];
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
            PlayerEvent.ShootingWeapon += OnShooting;
            PlayerEvent.UsedItem += OnItemUse;
            PlayerEvent.DroppedItem += OnDrop;
            PlayerEvent.ShotWeapon += OnShot;
            PlayerEvent.UpdatingEffect += OnReceivingEffect;
            PlayerEvent.ThrewProjectile += OnThrownProjectile;
            PlayerEvent.Dying += OnDying;
            PlayerEvent.ChangedItem += OnChangedItem;
            PlayerEvent.Hurting += OnHurting;
            PlayerEvent.InteractedDoor += OnDoorInteracted;
            PlayerEvent.InteractingDoor += OnDoorInteracting;
            PlayerEvent.UnlockingGenerator += OnGeneratorUnlock;
            PlayerEvent.InteractingLocker += OnLockerInteracting;
            PlayerEvent.Joined += OnVerified;
            PlayerEvent.PickedUpItem += OnPickup;
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.Left += OnLeft;
            PlayerEvent.FlippedCoin += OnFlippedCoin;
            PlayerEvent.ToggledFlashlight += OnToggledFlashlight;
            PlayerEvent.ToggledWeaponFlashlight += OnWeaponFlashlightToggled;
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
            InventoryExtensions.OnItemAdded += OnItemAdded;
        }

        public static void Unregister()
        {
            PlayerEvent.ShootingWeapon -= OnShooting;
            PlayerEvent.UsedItem -= OnItemUse;
            PlayerEvent.DroppedItem -= OnDrop;
            PlayerEvent.ShotWeapon -= OnShot;
            PlayerEvent.UpdatingEffect -= OnReceivingEffect;
            PlayerEvent.ThrewProjectile -= OnThrownProjectile;
            PlayerEvent.Dying -= OnDying;
            PlayerEvent.ChangedItem -= OnChangedItem;
            PlayerEvent.Hurting -= OnHurting;
            PlayerEvent.InteractedDoor -= OnDoorInteracted;
            PlayerEvent.InteractingDoor -= OnDoorInteracting;
            PlayerEvent.UnlockingGenerator -= OnGeneratorUnlock;
            PlayerEvent.InteractingLocker -= OnLockerInteracting;
            PlayerEvent.Joined -= OnVerified;
            PlayerEvent.PickedUpItem -= OnPickup;
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.Left -= OnLeft;
            PlayerEvent.FlippedCoin -= OnFlippedCoin;
            PlayerEvent.ToggledFlashlight -= OnToggledFlashlight;
            PlayerEvent.ToggledWeaponFlashlight -= OnWeaponFlashlightToggled;
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
            InventoryExtensions.OnItemAdded -= OnItemAdded;
        }

        private static void OnItemAdded(ReferenceHub hub, ItemBase itemBase, ItemPickupBase pickupBase)
        {
            Player player = Player.Get(hub);
            if (Utilities.TryGetSummonedCustomItem(itemBase.ItemSerial, out var item))
            {
                if (item.TryGetModule<PickupHintOverride>(out var hintoverride))
                {
                    player.SendHint(hintoverride.Hint.Replace("%name%", item.CustomItem.Name).Replace("%desc%", item.CustomItem.Description).Replace("%description%", item.CustomItem.Description), hintoverride.Duration);
                }
                else
                    item.HandlePickedUpDisplayHint(player);
            }

            if (SummonedAPICustomItem.TryGet(itemBase.ItemSerial, out var api))
                api.HandlePickedUpDisplayHint(player);
        }

        private static void OnProjectileThrew(PlayerThrewProjectileEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.ThrowableItem.Serial, out var summoned))
                summoned.OnThrew(ev);

            if (SummonedAPICustomItem.TryGet(ev.ThrowableItem.Serial, out var api))
                api.OnThrew(ev);
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
                        if (item.HasModule<NoCharge>())
                            ev.JailbirdItem.Base.SendRpc(JailbirdMessageType.ChargeFailed);

                        break;

                    case JailbirdMessageType.UpdateState:
                        if (item.CustomItem.CustomData is JailbirdData jailbird && !jailbird.AllowWearStateChanges && item.CustomItem.CustomItemType is CustomItemType.Jailbird)
                        {
                            JailbirdDeteriorationTracker.ReceivedStates[ev.JailbirdItem.Serial] = jailbird.WearState;                            
                            using (new AutosyncRpc(ev.JailbirdItem.Base.ItemId, out NetworkWriter writer))
                            {
                                writer.WriteByte(0);
                                writer.WriteByte((byte)jailbird.WearState);
                            }
                        }
                        
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
            while (player != null && player.IsAlive && player.HumeShield > 0)
            {
                player.HumeShield -= decayRate * Timing.DeltaTime;
                yield return Timing.WaitForOneFrame;
            }

            if (player != null && player.HumeShield < 0)
                player.HumeShield = 0;
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
                            if (customItem.CustomItem.CustomData is not SCP268Data hat)
                                break;

                            Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                            {
                                if (!hat.ApplyScp268Effect)
                                    ev.Player.DisableEffect<Invisible>();

                                if (hat.ApplyScp268Effect)
                                {
                                    ev.Player.EnableEffect<Invisible>(1, hat.Duration, false);
                                    CustomScp268Effects.TryAdd(ev.Player);
                                }

                                if (hat.OneTimeUse)
                                    ev.UsableItem.DropItem().Destroy();
                            });

                            break;
                        
                        case ItemType.SCP207 or ItemType.AntiSCP207 when customItem.CustomItem.CustomData is SCP207Data cola:
                            if (!cola.Apply207Effect)
                            {
                                ev.IsAllowed = false;
                                ev.ContinueProcess = false;
                            }
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
            }
            else
            {
                LogManager.Info("Selected candy is not tracked as serialized.");
            }
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

                if (customItem.TryGetModule<PickupHintOverride>(out var hintoverride))
                {
                    ev.Player.SendHint(hintoverride.Hint.Replace("%name%", customItem.CustomItem.Name).Replace("%desc%", customItem.CustomItem.Description).Replace("%description%", customItem.CustomItem.Description), hintoverride.Duration);
                }
                else
                    customItem.HandleSelectedDisplayHint(ev.Player);

                customItem?.LoadBadge(ev.Player);

                Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                {
                    if (customItem.CustomItem.CustomItemType is CustomItemType.Light && customItem.Item is LightItem lightSource)
                        lightSource.IsEmitting = false;
                });

                if (customItem.CustomItem.Item == ItemType.GunSCP127 && customItem.CustomItem.CustomItemType == CustomItemType.SCPItem)
                {
                    ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(customItem.Item.Base);
                    StartHumeShieldRegen(ev.Player, data, tier, customItem);
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
            if (_humeShieldRegenCoroutine.TryGetValue(player, out CoroutineHandle handle))
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
                    player.HumeShieldRegenRate = regenRate;

                if (player.CurrentItem == null || player.CurrentItem.Serial != customItem.Serial)
                    yield break;

                yield return Timing.WaitForOneFrame;
            }
        }

        internal static IEnumerator<float> HumeShieldRegeneration(Player player, ISCP127Data data, Scp127Tier tier, SummonedCustomItem customItem)
        {
            float regenRate = tier switch
            {
                Scp127Tier.Tier1 => data.Tier1ShieldRegenRate,
                Scp127Tier.Tier2 => data.Tier2ShieldRegenRate,
                Scp127Tier.Tier3 => data.Tier3ShieldRegenRate,
                _ => 0f
            };

            float damagePause = tier switch
            {
                Scp127Tier.Tier1 => data.Tier1ShieldOnDamagePause,
                Scp127Tier.Tier2 => data.Tier2ShieldOnDamagePause,
                Scp127Tier.Tier3 => data.Tier3ShieldOnDamagePause,
                _ => 0f
            };

            while (player != null && player.IsAlive)
            {
                if (player.CurrentItem == null || player.CurrentItem.Serial != customItem.Serial)
                {
                    player.HumeShieldRegenRate = 0f;
                    yield break;
                }

                if (_damageTimes.TryGetValue(player, out long time))
                {
                    long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
                    player.HumeShieldRegenRate = (elapsed >= damagePause) ? regenRate : 0f;
                }
                else
                    player.HumeShieldRegenRate = regenRate;

                yield return Timing.WaitForOneFrame;
            }

            player?.HumeShieldRegenRate = 0f;
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
            if (customItem.TryGetModule<PickupHintOverride>(out var hintoverride))
            {
                ev.Player.SendHint(hintoverride.Hint.Replace("%name%", customItem.CustomItem.Name).Replace("%desc%", customItem.CustomItem.Description).Replace("%description%", customItem.CustomItem.Description), hintoverride.Duration);
            }
            else
                customItem.HandlePickedUpDisplayHint(ev.Player);
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
        }

        public static void OnToggledFlashlight(PlayerToggledFlashlightEventArgs ev)
        {
            if (ev.LightItem == null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.LightItem.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.CustomItem.CustomItemType is CustomItemType.Item)
                customItem.HandleEvent(ev.Player, ItemEvents.Use, ev.LightItem.Serial);
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
        }

        public static void OnDrop(PlayerDroppedItemEventArgs ev)
        {
            LogManager.Debug($"Item Dropped: {ev.Pickup.Type} - Serial: {ev.Pickup.Serial}");
            if (ev.Pickup == null)
                return;

            LogManager.Debug($"Pickup is not null");
            foreach (SummonedCustomItem item in SummonedCustomItem.List.ToArray())
            {
                LogManager.Debug($"{item.Serial}");
            }
            if (SummonedAPICustomItem.TryGet(ev.Pickup.Serial, out var summonedItem))
            {
                summonedItem.OnDrop(ev);
                summonedItem?.ResetBadge(ev.Player);
                StopHumeShieldRegen(ev.Player);
                
                ev.Pickup.GameObject.transform.localScale = summonedItem.CustomItem.Scale;
                ev.Pickup.Weight = summonedItem.CustomItem.Weight;

                if (summonedItem.CustomItem is ToolGun)
                    summonedItem.Destroy();
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem summonedCustomItem))
                return;

            LogManager.Debug($"Pickup is a CustomItem");
            summonedCustomItem.OnDrop(ev);
            summonedCustomItem?.ResetBadge(ev.Player);
            StopHumeShieldRegen(ev.Player);

            try
            {
                ev.Pickup.GameObject.transform.localScale = summonedCustomItem.CustomItem.Scale;
                ev.Pickup.Weight = summonedCustomItem.CustomItem.Weight;
            }
            catch (Exception ex)
            {
                LogManager.Silent($"{summonedCustomItem.CustomItem.Name} - {summonedCustomItem.CustomItem.Id}");
                LogManager.Error($"Couldnt set CustomItem Pickup Scale or CustomItem Pickup Weight\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
            }
        }

        public static void OnDying(PlayerDyingEventArgs ev)
        {
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

            if (ev.FirearmItem.ActionModule is AutomaticActionModule actionModule)
            {
                actionModule._serverQueuedRequests.Clear();
                actionModule._clientQueuedShots.Clear();
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