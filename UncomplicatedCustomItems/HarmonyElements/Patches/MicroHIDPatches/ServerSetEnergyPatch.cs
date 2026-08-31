using HarmonyLib;
using InventorySystem.Items.MicroHID.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(EnergyManagerModule), nameof(EnergyManagerModule.ServerSetEnergy))]
    internal static class ServerSetEnergyPatch
    {
        private static bool Prefix(EnergyManagerModule __instance)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.MicroHid.ItemSerial, out SummonedCustomItem? customItem) && customItem != null && customItem.CustomItem.CustomData is MicroHIDData md)
            {
                if (md.InfiniteEnergy)
                    return false;
            }
            
            return true;
        }
    }
}