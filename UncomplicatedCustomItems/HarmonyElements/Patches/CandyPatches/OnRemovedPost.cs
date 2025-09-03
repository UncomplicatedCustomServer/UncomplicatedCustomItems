using System;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.OnRemoved))]
    public static class OnRemovedPost
    {
        [HarmonyPostfix]
        public static void OnRemovedPostfix(Scp330Bag __instance, ItemPickupBase pickup)
        {
            try
            {
                if (pickup is Scp330Pickup scpPickup)
                {
                    LogManager.Debug($"{nameof(OnRemovedPostfix)}");
                    CandySerializationManager.MoveBagToPickup(scpPickup, __instance);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"[CandySerialization] OnRemovedPost error: {ex}");
            }
        }

    }
}