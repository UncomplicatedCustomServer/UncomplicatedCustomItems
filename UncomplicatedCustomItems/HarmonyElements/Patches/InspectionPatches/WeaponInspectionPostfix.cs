using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(SimpleInspectorModule), nameof(SimpleInspectorModule.ServerProcessCmd))]
    public static class WeaponInspectionPostfix
    {
        [HarmonyPostfix]
        public static void Postfix(SimpleInspectorModule __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return;

            customItem.HandleEvent(Player.Get(__instance.Firearm.Owner), ItemEvents.Inspect, __instance.Firearm.ItemSerial);
        }
    }
}