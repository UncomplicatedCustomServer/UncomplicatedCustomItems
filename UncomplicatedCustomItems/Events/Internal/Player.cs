using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using MEC;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using EventSource = Exiled.Events.Handlers.Player;
using Exiled.API.Features;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal static class Player
    {
        public static void Register()
        {
            EventSource.Hurting += SetDamageFromCustomWeaponOnHurting;
            EventSource.ItemAdded += ShowItemInfoOnItemAdded;
            EventSource.DroppedItem += DroppedItemEvent;
            EventSource.ChangedItem += ChangeItemInHand;
            EventSource.ChangingItem += ChangingItemInHand;
            EventSource.UsingItemCompleted += OnItemUsingCompleted;
            EventSource.TogglingNoClip += NoclipButton;
            EventSource.Died += DeathEvent;
        }

        public static void Unregister()
        {
            EventSource.Hurting -= SetDamageFromCustomWeaponOnHurting;
            EventSource.ItemAdded -= ShowItemInfoOnItemAdded;
            EventSource.DroppedItem -= DroppedItemEvent;
            EventSource.ChangedItem -= ChangeItemInHand;
            EventSource.ChangingItem -= ChangingItemInHand;
            EventSource.UsingItemCompleted -= OnItemUsingCompleted;
            EventSource.Died += DeathEvent;
        }

        private static void DroppedItemEvent(DroppedItemEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem Item))
                Item.OnDrop(ev);
                Item?.ResetBadge(ev.Player);
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
                return;

            if (ev.Attacker.CurrentItem is not Firearm)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
                return;

            if (Item.CustomItem.CustomItemType != CustomItemType.Weapon)
                return;

            if (Item.CustomItem.CustomData is not IWeaponData WeaponData)
                return;

            ev.DamageHandler.Damage = WeaponData.Damage;
        }

        private static void OnItemUsingCompleted(UsingItemCompletedEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
                return;

            if (Item is null)
                return;

            Item.HandleEvent(ev.Player, ItemEvents.Use);

            if (Item.CustomItem.Reusable)
                ev.IsAllowed = false;
        }

        private static void ChangeItemInHand(ChangedItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.HandleSelectedDisplayHint();
            item?.LoadBadge(ev.Player);
        }
        private static void ChangingItemInHand(ChangingItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem item))
                return;

            item?.ResetBadge(ev.Player);
        }
        private static void DeathEvent(DiedEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
                return;

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

            Item?.HandleEvent(ev.Player, ItemEvents.Noclip);
        }
    }
}
