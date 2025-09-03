using System;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
    public static class TryRemovePost
    {
        [HarmonyPostfix]
        public static void TryRemovePostfix(Scp330Bag __instance, int index, ref CandyKindID __result)
        {
            try
            {
                if (__result == CandyKindID.None)
                    return;

                LogManager.Debug($"{nameof(TryRemovePostfix)}");
                CandySerializationManager.RemoveCandyFromBagAt(__instance, index);
            }
            catch (Exception ex)
            {
                LogManager.Error($"[CandySerialization] TryRemovePost error: {ex}");
            }
        }
    }
}