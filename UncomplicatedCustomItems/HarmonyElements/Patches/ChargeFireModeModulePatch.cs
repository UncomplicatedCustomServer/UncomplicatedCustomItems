using HarmonyLib;
using InventorySystem.Items.MicroHID.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(ChargeFireModeModule), nameof(ChargeFireModeModule.ServerExplode))]
    internal static class ChargeFireModeModulePatch
    {
        private static bool Prefix(ChargeFireModeModule __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.MicroHid.ItemSerial, out SummonedCustomItem customItem))
                return true;

            if (!(customItem.CustomItem.CustomData is IMicroHIDData data))
                return true;

            if (!data.CanExplode)
                return false;

            return true;
        }
    }
}