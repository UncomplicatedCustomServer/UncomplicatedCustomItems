using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using EventSource = Exiled.Events.Handlers.Player;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal static class Player
    {
        public static void Register()
        {
            EventSource.UsingItem += CancelUsingCustomItemOnUsingItem;
            EventSource.Hurting += SetDamageFromCustomWeaponOnHurting;
            EventSource.ItemAdded += ShowItemInfoOnItemAdded;
            EventSource.DroppedItem += DroppedItemEvent;
            EventSource.ChangedItem += ChangeItemInHand;
        }

        public static void Unregister()
        {
            EventSource.UsingItem -= CancelUsingCustomItemOnUsingItem;
            EventSource.Hurting -= SetDamageFromCustomWeaponOnHurting;
            EventSource.ItemAdded -= ShowItemInfoOnItemAdded;
            EventSource.DroppedItem -= DroppedItemEvent;
            EventSource.ChangedItem -= ChangeItemInHand;
        }

        private static void DroppedItemEvent(DroppedItemEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem Item))
            {
                Item.OnDrop(ev);
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
            }
        }

        /// <summary>
        /// Set damage if weapon is custom item
        /// </summary>
        /// <param name="ev"></param>
        private static void SetDamageFromCustomWeaponOnHurting(HurtingEventArgs ev)
        {
            if (ev.DamageHandler.Type is not Exiled.API.Enums.DamageType.Firearm)
            {
                return;
            }

            if (ev.Attacker.CurrentItem is not Firearm)
            {
                return;
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
            {
                return;
            }

            if (Item.CustomItem.CustomItemType != CustomItemType.Weapon)
            {
                return;
            }

            if (Item.CustomItem.CustomData is not IWeaponData WeaponData)
            {
                return;
            }

            ev.DamageHandler.Damage = WeaponData.Damage;
        }

        /// <summary>
        /// Cancel using if it is custom item
        /// </summary>
        /// <param name="ev"></param>
        private static void CancelUsingCustomItemOnUsingItem(UsingItemEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                return;
            }

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
            {
                ev.IsAllowed = false;
                Item.HandleEvent(ev.Player, ItemEvents.Use);
            }
        }

        private static void ChangeItemInHand(ChangedItemEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
            {
                return;
            }

            Item.HandleSelectedDisplayHint();
        }
    }
}
