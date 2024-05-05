﻿using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Usables;
using MEC;
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
            EventSource.Hurting += SetDamageFromCustomWeaponOnHurting;
            EventSource.ItemAdded += ShowItemInfoOnItemAdded;
            EventSource.DroppedItem += DroppedItemEvent;
            EventSource.ChangedItem += ChangeItemInHand;
            EventSource.UsingItemCompleted += OnItemUsingCompleted;
            EventSource.TogglingNoClip += NoclipButton;
        }

        public static void Unregister()
        {
            EventSource.Hurting -= SetDamageFromCustomWeaponOnHurting;
            EventSource.ItemAdded -= ShowItemInfoOnItemAdded;
            EventSource.DroppedItem -= DroppedItemEvent;
            EventSource.ChangedItem -= ChangeItemInHand;
            EventSource.UsingItemCompleted -= OnItemUsingCompleted;
            EventSource.TogglingNoClip -= NoclipButton;
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

        private static void OnItemUsingCompleted(UsingItemCompletedEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Item))
            {
                return;
            }

            if (Item is null)
            {
                // Non si sa mai
                return;
            }

            if (Item.CustomItem.CustomItemType is CustomItemType.Medikit)
            {
                // Do the medikit thing
                if (Item.CustomItem.CustomData is not IMedikitData Data)
                {
                    return;
                }
                ev.IsAllowed = false;
                ev.Player.Heal(Data.Health, Data.MoreThanMax);
                Item.Destroy();
            }
            else if (Item.CustomItem.CustomItemType is CustomItemType.Painkillers)
            {
                // Do the painkillers thing
                if (Item.CustomItem.CustomData is not IPainkillersData Data)
                {
                    return;
                }
                ev.IsAllowed = false;
                Timing.RunCoroutine(Utilities.PainkillersCoroutine(ev.Player, Data));
                Item.Destroy();
            } 
            else if (Item.CustomItem.CustomItemType is CustomItemType.Item)
            {
                Item.HandleEvent(ev.Player, ItemEvents.Use);
            }
        }

        private static void ChangeItemInHand(ChangedItemEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
            {
                return;
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
            {
                return;
            }

            Item?.HandleSelectedDisplayHint();
        }

        private static void NoclipButton(TogglingNoClipEventArgs ev)
        {
            if (ev.Player.CurrentItem is null)
            {
                return;
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem Item))
            {
                return;
            }

            Item?.HandleEvent(ev.Player, ItemEvents.Noclip);
        }
    }
}
