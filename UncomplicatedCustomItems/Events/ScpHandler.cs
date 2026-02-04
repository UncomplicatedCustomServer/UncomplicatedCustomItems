using LabApi.Events.Arguments.Scp914Events;
using System;
using UncomplicatedCustomItems.API.Extensions;
using Scp914Event = LabApi.Events.Handlers.Scp914Events;

namespace UncomplicatedCustomItems.Events
{
    internal class ScpHandler
    {
        public static void Register()
        {
            Scp914Event.ProcessingPickup += OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem += OnItemUpgrade;
        }

        public static void Unregister()
        {
            Scp914Event.ProcessingPickup -= OnPickupUpgrade;
            Scp914Event.ProcessingInventoryItem -= OnItemUpgrade;
        }

        public static void OnPickupUpgrade(Scp914ProcessingPickupEventArgs ev)
        {
            if (ev.Pickup.IsCustomItem() || ev.Pickup.IsSummonedAPICustomItem())
            {
                ev.IsAllowed = false;
                ev.Pickup.Position = ev.NewPosition;
            }
        }

        public static void OnItemUpgrade(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (ev.Item.IsSummonedCustomItem() || ev.Item.IsSummonedAPICustomItem())
                ev.IsAllowed = false;
        }
    }
}