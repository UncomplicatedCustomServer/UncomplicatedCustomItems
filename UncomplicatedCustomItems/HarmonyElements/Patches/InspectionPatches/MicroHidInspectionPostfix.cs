using HarmonyLib;
using InventorySystem.Items.MicroHID.Modules;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(DrawAndInspectorModule), nameof(DrawAndInspectorModule.ServerProcessCmd))]
    public static class MicroHidInspectionPostfix
    {
        [HarmonyPostfix]
        public static void Postfix(DrawAndInspectorModule __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                return;

            customItem.HandleEvent(Player.Get(__instance.MicroHid.Owner), ItemEvents.Inspect, __instance.ItemSerial);
        }
    }
}