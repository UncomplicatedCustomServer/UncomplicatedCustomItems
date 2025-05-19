using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using EventSource = Exiled.Events.Handlers.Player;
using Exiled.API.Extensions;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using Exiled.API.Enums;
using MapEventSource = Exiled.Events.Handlers.Map;
using Exiled.Events.EventArgs.Map;
using Exiled.API.Features.Items;
using Exiled.API.Features.Core.UserSettings;
using InventorySystem.Items.Firearms.Modules.Scp127;
using MEC;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Wrappers;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal static class Player
    {   //EventSource.EVENT += EVENTNAME
        public static void Register()
        {
            EventSource.ItemAdded += ShowItemInfoOnItemAdded;
            EventSource.DroppedItem += DroppedItemEvent;
            EventSource.ChangedItem += ChangeItemInHand;
            EventSource.ChangingItem += ChangingItemInHand;
            EventSource.UsingItemCompleted += OnItemUsingCompleted;
            EventSource.TogglingNoClip += NoclipButton;
            EventSource.Dying += DeathEvent;
            EventSource.ChangingRole += RoleChangeEvent;
            EventSource.ThrownProjectile += ThrownProjectile;
            EventSource.Shot += Damaged;
            MapEventSource.ExplodingGrenade += GrenadeExploded;
        }
        // EventSource.EVENT -= EVENTNAME 
        public static void Unregister()
        {
            EventSource.ItemAdded -= ShowItemInfoOnItemAdded;
            EventSource.DroppedItem -= DroppedItemEvent;
            EventSource.ChangedItem -= ChangeItemInHand;
            EventSource.ChangingItem -= ChangingItemInHand;
            EventSource.UsingItemCompleted -= OnItemUsingCompleted;
            EventSource.Dying -= DeathEvent;
            EventSource.ChangingRole -= RoleChangeEvent;
            EventSource.ThrownProjectile -= ThrownProjectile;
            EventSource.Shot -= Damaged;
            EventSource.TogglingNoClip -= NoclipButton;
            MapEventSource.ExplodingGrenade -= GrenadeExploded;
        }

        public static void Damaged(ShotEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                IWeaponData WeaponData = CustomItem.CustomItem.CustomData as IWeaponData;
                if (ItemExtensions.GetCategory(CustomItem.Item.Type) == ItemCategory.Firearm)
                {
                    if (WeaponData.EnableFriendlyFire)
                    {
                        if (ev.Target != null)
                        {
                            ev.Target.Hurt(WeaponData.Damage, DamageType.Firearm);
                            ev.Player.ShowHitMarker(1);
                        }
                    }
                }
            }
        }

        private static void GrenadeExploded(ExplodingGrenadeEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem Item))
                return;

            Item?.HandleEvent(ev.Player, ItemEvents.Detonation, ev.Projectile.Serial); // Untested
        }

        private static void DroppedItemEvent(DroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem Item))
                return;

                Item?.OnDrop(ev);
                Item.ResetBadge(ev.Player);
            if (Item.HasModule(Enums.CustomFlags.ToolGun))
            {
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                EventHandler.StopRelativePosCoroutine(ev.Player);
            }
            
            EventHandler.StopHumeShieldRegen(ev.Player);
        }

        /// <summary>
        /// Show item name if it is custom item
        /// </summary>
        /// <param name="ev"></param>
        private static void ShowItemInfoOnItemAdded(ItemAddedEventArgs ev)
        {
            if (ev.Item is null)
                return;
            if (ev.Player is null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
            {
                Item.OnPickup(ev);
                Item.HandlePickedUpDisplayHint();
            }
        }

        private static void OnItemUsingCompleted(UsingItemCompletedEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Item == null)
                return;
            if (ev.Usable == null)
                return;
                
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
                return;

            if (Item is null)
                return;
            
            Item.HandleEvent(ev.Player, ItemEvents.Use, ev.Item.Serial);

            Item?.ResetBadge(ev.Player);

            if (Item.CustomItem.Reusable)
                ev.IsAllowed = false;
        }

        private static void ChangeItemInHand(ChangedItemEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (ev.Item is null)
                return;

            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item.HandleSelectedDisplayHint();
            item.LoadBadge(ev.Player);
        }

        private static void ChangingItemInHand(ChangingItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
            {
                if (EventHandler.EquipedKeycards.ContainsKey(ev.Player.CurrentItem.Serial))
                    EventHandler.EquipedKeycards.Remove(ev.Player.CurrentItem.Serial);
                return;
            }
            if (EventHandler.EquipedKeycards.ContainsKey(item.Serial))
                EventHandler.EquipedKeycards.Remove(item.Serial);

            item.ResetBadge(ev.Player);

            if (item.HasModule(Enums.CustomFlags.ToolGun))
            {
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                EventHandler.StopRelativePosCoroutine(ev.Player);
            }

            if (item.Item.Type == ItemType.GunSCP127 && item.CustomItem.CustomItemType == CustomItemType.SCPItem)
            {
                ISCP127Data data = item.CustomItem.CustomData as ISCP127Data;
                Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(item.Item.Base);
                if (tier == Scp127Tier.Tier1)
                {
                    ev.Player.HumeShieldRegenerationMultiplier = 0f;
                    Timing.RunCoroutine(DecayRate(ev.Player, data.Tier1ShieldDecayRate));
                }
                else if (tier == Scp127Tier.Tier2)
                {
                    ev.Player.HumeShieldRegenerationMultiplier = 0f;
                    Timing.RunCoroutine(DecayRate(ev.Player, data.Tier2ShieldDecayRate));
                }
                else if (tier == Scp127Tier.Tier3)
                {
                    ev.Player.HumeShieldRegenerationMultiplier = 0f;
                    Timing.RunCoroutine(DecayRate(ev.Player, data.Tier3ShieldDecayRate));
                }
                else
                    LogManager.Error($"{item.CustomItem.Name} - {item.Serial} has no tier?");
                EventHandler.StopHumeShieldRegen(ev.Player);
            }
        }

        internal static IEnumerator<float> DecayRate(Exiled.API.Features.Player player, float DecayRate)
        {
            for (; ; )
            {
                if (player.HumeShield >= 0)
                {
                    player.HumeShield -= Time.deltaTime * DecayRate;
                    yield return Timing.WaitForOneFrame;
                }
                else
                {
                    yield break;
                }
            }
        }

        private static void DeathEvent(DyingEventArgs ev)
        {
            if (!ev.Player.IsConnected)
                return;

            if (ev.Player == null)
                return;

            foreach (Item item in ev.Player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customitem))
                {
                    customitem.OnDied(ev, customitem);
                    customitem?.ResetBadge(ev.Player);
                    if (customitem.HasModule(Enums.CustomFlags.ToolGun))
                    {
                        SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                        EventHandler.StopRelativePosCoroutine(ev.Player);
                    }
                }
            }

            EventHandler.StopHumeShieldRegen(ev.Player);
        }

        private static void RoleChangeEvent(ChangingRoleEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!ev.Player.IsConnected)
                return;
                
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
            if (item.HasModule(Enums.CustomFlags.ToolGun))
            {
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
                EventHandler.StopRelativePosCoroutine(ev.Player);
            }

            EventHandler.StopHumeShieldRegen(ev.Player);
        }

        private static void ThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
        }

        private static void NoclipButton(TogglingNoClipEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
                return;

            Item?.HandleEvent(ev.Player, ItemEvents.Noclip, ev.Player.CurrentItem.Serial);

            if (Plugin.Instance.Config.Debug == true)
            {
                if (ev.Player.RemoteAdminPermissions == PlayerPermissions.PlayersManagement)
                {
                    Item.ShowDebugUi(ev.Player);
                }
            }
        }
    }
}