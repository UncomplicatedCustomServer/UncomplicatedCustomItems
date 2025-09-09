using HarmonyLib;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using InventorySystem.Items.Usables.Scp1344;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(Scp1344NetworkHandler), nameof(Scp1344NetworkHandler.TryInspect))]
    public static class Scp1344InspectionPostfix
    {
        [HarmonyPostfix]
        public static void Postfix(InventorySystem.Items.Usables.Scp1344.Scp1344Item __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                return;

            customItem.HandleEvent(Player.Get(__instance.Owner), ItemEvents.Inspect, __instance.ItemSerial);
        }
    }
}