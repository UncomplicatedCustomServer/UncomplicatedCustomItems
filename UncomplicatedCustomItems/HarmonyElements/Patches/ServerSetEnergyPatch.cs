using HarmonyLib;
using InventorySystem.Items.MicroHID.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(EnergyManagerModule), nameof(EnergyManagerModule.ServerSetEnergy))]
    internal static class ServerSetEnergyPatch
    {
        private static bool Prefix(EnergyManagerModule __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.MicroHid.ItemSerial, out SummonedCustomItem customItem))
                return true;

            if (!(customItem.CustomItem.CustomData is IMicroHIDData data))
                return true;

            if (data.InfiniteEnergy)
                return false;

            return true;
        }
    }
}