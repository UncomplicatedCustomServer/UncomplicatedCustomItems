using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MapGeneration;
using UncomplicatedCustomItems.API.Extensions;
using UnityEngine;
using Utf8Json.Internal.DoubleConversion;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public class CustomItemUtils
    {
        public static Pickup FindTargetPickupInRoom(Room room, BaseCustomItem customItem)
        {
            List<Pickup> pickupsInRoom = [];
            if (customItem.ForceSameItemType)
            {
                pickupsInRoom = Pickup.List.Where(pickup =>
                    pickup.Room == room && pickup.Type == customItem.Item && !SummonedBaseCustomItem.TryGet(pickup.Serial, out _)).ToList();
            }
            else
            {
                pickupsInRoom = Pickup.List.Where(pickup =>
                    pickup.Room == room && !SummonedBaseCustomItem.TryGet(pickup.Serial, out _)).ToList();
            }

            return FilterAndSelectPickup(pickupsInRoom, customItem);
        }


        public static Pickup FilterAndSelectPickup(List<Pickup> pickups, BaseCustomItem customItem)
        {
            if (customItem.ForceSameItemType)
                pickups = pickups.Where(pickup => pickup.Type == customItem.Item).ToList();

            return pickups.Count > 0 ? pickups.RandomItem() : null;
        }
    }
}