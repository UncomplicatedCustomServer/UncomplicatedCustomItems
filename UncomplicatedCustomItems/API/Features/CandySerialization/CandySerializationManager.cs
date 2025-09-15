using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items.Usables.Scp330;

namespace UncomplicatedCustomItems.API.Features.CandySerialization
{
    public static class CandySerializationManager
    {
        private static readonly Dictionary<Scp330Bag, List<SerializedCandy>> BagSerializedByInstance = [];
        private static readonly Dictionary<Scp330Pickup, List<SerializedCandy>> PickupSerializedByInstance = [];

        public static List<SerializedCandy> EnsureBagList(Scp330Bag bag)
        {
            if (bag == null)
                throw new ArgumentNullException(nameof(bag));
                
            if (!BagSerializedByInstance.TryGetValue(bag, out List<SerializedCandy> list))
            {
                list = [];
                BagSerializedByInstance[bag] = list;
            }

            return list;
        }

        public static void AddCandyToBag(Scp330Bag bag, CandyKindID kind)
        {
            List<SerializedCandy> list = EnsureBagList(bag);
            SerializedCandy sc = new()
            {
                Id = Guid.NewGuid(),
                CandyType = kind,
                AddedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                BagSerial = bag.ItemSerial,
                IsCustom = false
            };
            list.Add(sc);
        }

        public static void ReplaceLastCandyInBag(Scp330Bag bag, SerializedCandy replacement)
        {
            if (bag == null)
                throw new ArgumentNullException(nameof(bag));
            if (replacement == null)
                throw new ArgumentNullException(nameof(replacement));

            List<SerializedCandy> list = EnsureBagList(bag);
            replacement.BagSerial = bag.ItemSerial;

            if (list.Count == 0)
            {
                list.Add(replacement);
                return;
            }
            
            list[list.Count - 1] = replacement;
        }

        public static SerializedCandy? RemoveCandyFromBagAt(Scp330Bag bag, int index)
        {
            List<SerializedCandy> list = EnsureBagList(bag);
            if (index < 0 || index >= list.Count)
                return null;

            SerializedCandy sc = list[index];
            list.RemoveAt(index);
            if (list.Count == 0)
                BagSerializedByInstance.Remove(bag);

            return sc;
        }

        public static bool TryGetCandyInBag(Scp330Bag bag, int index, out SerializedCandy? candy)
        {
            candy = null;
            if (bag == null)
                return false;

            List<SerializedCandy> list = EnsureBagList(bag);
            if (index < 0 || index >= list.Count)
                return false;

            candy = list[index];
            return candy != null;
        }

        public static void MoveBagToPickup(Scp330Pickup pickup, Scp330Bag bag)
        {
            if (pickup == null)
                throw new ArgumentNullException(nameof(pickup));
            if (bag == null)
                throw new ArgumentNullException(nameof(bag));
            if (!BagSerializedByInstance.TryGetValue(bag, out List<SerializedCandy> list))
                return;

            List<SerializedCandy> copy = list.Select(s => new SerializedCandy
            {
                Id = s.Id,
                CandyType = s.CandyType,
                AddedTimestamp = s.AddedTimestamp,
                BagSerial = bag.ItemSerial,
                IsCustom = s.IsCustom,
                CustomItemId = s.CustomItemId
            }).ToList();

            PickupSerializedByInstance[pickup] = copy;
            BagSerializedByInstance.Remove(bag);
        }

        public static List<SerializedCandy>? GetPickupSerialized(Scp330Pickup pickup)
        {
            if (pickup == null)
                return null;

            PickupSerializedByInstance.TryGetValue(pickup, out List<SerializedCandy> list);
            return list;
        }

        public static bool PickupHasSerialized(Scp330Pickup pickup)
        {
            List<SerializedCandy> l = GetPickupSerialized(pickup);
            return l != null && l.Count > 0;
        }

        public static SerializedCandy? PopFirstPickupSerialized(Scp330Pickup pickup)
        {
            List<SerializedCandy> list = GetPickupSerialized(pickup);
            if (list == null || list.Count == 0)
                return null;

            SerializedCandy s = list[0];
            list.RemoveAt(0);
            if (list.Count == 0)
                PickupSerializedByInstance.Remove(pickup);

            return s;
        }

        public static void RemovePickupMapping(Scp330Pickup pickup) => PickupSerializedByInstance.Remove(pickup);
    }
}
