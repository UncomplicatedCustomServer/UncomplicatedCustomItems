using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using EventSource = Exiled.Events.Handlers.Player;
using UncomplicatedCustomItems.API.Features.CustomModules;
using UncomplicatedCustomItems.Enums;
using Exiled.API.Extensions;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using Exiled.API.Enums;

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
        }

        public static void Damaged(ShotEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                IWeaponData WeaponData = CustomItem.CustomItem.CustomData as IWeaponData;
                if (ItemExtensions.GetCategory(CustomItem.Item.Type) == ItemCategory.Firearm)
                {
                    if (WeaponData.EnableFriendlyFire == true)
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


        private static void DroppedItemEvent(DroppedItemEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem Item))
            {
                Item.OnDrop(ev);
                Item.ResetBadge(ev.Player);
                Item?.UnloadItemFlags();
            }
        }

        /// <summary>
        /// Show item name if it is custom item
        /// </summary>
        /// <param name="ev"></param>
        private static void ShowItemInfoOnItemAdded(ItemAddedEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
            {
                Item.OnPickup(ev);
                Item.HandlePickedUpDisplayHint();
                CustomModule.Load((CustomFlags)Item.CustomItem.CustomFlags, Item);
                Item.ReloadItemFlags();
                Item.LoadItemFlags();
                SummonedCustomItem.Register(Item.CustomItem.FlagSettings);
                SummonedCustomItem.GetAllFlagSettings();
            }
        }

        private static void OnItemUsingCompleted(UsingItemCompletedEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
                return;

            if (Item is null)
                return;
            
            Item.HandleEvent(ev.Player, ItemEvents.Use);

            Item?.ResetBadge(ev.Player);
            Item.UnloadItemFlags();
            SummonedCustomItem.ClearAllFlagSettings();

            if (Item.CustomItem.Reusable)
                ev.IsAllowed = false;
        }

        private static void ChangeItemInHand(ChangedItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item.HandleSelectedDisplayHint();
            item.LoadBadge(ev.Player);
            CustomModule.Load((CustomFlags)item.CustomItem.CustomFlags, item);
            item.ReloadItemFlags();
            item.LoadItemFlags();
            SummonedCustomItem.Register(item.CustomItem.FlagSettings);
            SummonedCustomItem.GetAllFlagSettings();
        }
        private static void ChangingItemInHand(ChangingItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item.ResetBadge(ev.Player);
            item.ReloadItemFlags();
            item.UnloadItemFlags();
            SummonedCustomItem.ClearAllFlagSettings();
        }
        private static void DeathEvent(DyingEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
            item.UnloadItemFlags();
            SummonedCustomItem.ClearAllFlagSettings();
        }
        private static void RoleChangeEvent(ChangingRoleEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;
                
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
            item.UnloadItemFlags();
            SummonedCustomItem.ClearAllFlagSettings();
        }
        private static void ThrownProjectile(ThrownProjectileEventArgs ev)
        {

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
            item.UnloadItemFlags();
            SummonedCustomItem.ClearAllFlagSettings();
        }
        private static void NoclipButton(TogglingNoClipEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
                return;

            Item?.HandleEvent(ev.Player, ItemEvents.Noclip);
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