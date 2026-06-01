using HarmonyLib;
using InventorySystem.Items.MicroHID.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(ChargeFireModeModule), nameof(ChargeFireModeModule.ServerExplode))]
    internal static class ChargeFireModeModulePatch
    {
        private static bool Prefix(ChargeFireModeModule __instance)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.MicroHid.ItemSerial, out SummonedCustomItem? customItem) && customItem != null && customItem.CustomItem.CustomData is IMicroHIDData md)
            {
                if (!md.CanExplode)
                    return false;
            }

            return true;
        }
    }
}