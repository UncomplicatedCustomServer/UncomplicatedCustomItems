using System;
using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.ServerProcessPickup))]
    public static class ServerProcessPickupPre
    {
        [HarmonyPrefix]
        public static bool ServerProcessPickupPrefix(ref bool __result, ReferenceHub ply, Scp330Pickup pickup, out Scp330Bag bag)
        {
            bag = null;
            try
            {
                bool got = Scp330Bag.TryGetBag(ply, out bag);

                if (!got)
                    return true;

                if (pickup == null || !CandySerializationManager.PickupHasSerialized(pickup))
                    return true;

                bool result = false;

                while (CandySerializationManager.PickupHasSerialized(pickup))
                {
                    SerializedCandy serialCandy = CandySerializationManager.PopFirstPickupSerialized(pickup);
                    if (serialCandy == null)
                        break;

                    bool added = bag.TryAddSpecific(serialCandy.CandyType);
                    if (!added)
                    {
                        List<SerializedCandy> pickupList = CandySerializationManager.GetPickupSerialized(pickup);
                        if (pickupList != null)
                            pickupList.Insert(0, serialCandy);

                        break;
                    }

                    LogManager.Debug($"{nameof(ServerProcessPickupPrefix)}");
                    CandySerializationManager.ReplaceLastCandyInBag(bag, serialCandy);
                    result = true;
                }

                if (!CandySerializationManager.PickupHasSerialized(pickup))
                    CandySerializationManager.RemovePickupMapping(pickup);

                __result = result;
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"[CandySerialization] ServerProcessPickupPre error: {ex}");
                return true;
            }
        }
    }
}
