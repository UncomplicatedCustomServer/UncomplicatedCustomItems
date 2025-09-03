using System;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomItems.API.Features.CandySerialization;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.ServerOnUsingCompleted))]
    public static class ServerOnUsingCompletedPost
    {
        [HarmonyPostfix]
        public static void ServerOnUsingCompletedPostfix(Scp330Bag __instance)
        {
            try
            {
                int selected = __instance.SelectedCandyId;
                if (selected < 0)
                    return;

                LogManager.Debug($"{nameof(ServerOnUsingCompletedPostfix)}");
                CandySerializationManager.RemoveCandyFromBagAt(__instance, selected);
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(ServerOnUsingCompletedPostfix)}: [CandySerialization error: {ex}");
            }
        }
    }
}