using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.MicroHID.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AudioManagerModule), nameof(AudioManagerModule.UpdateInstance))]
    internal static class MicroHIDAudioManagerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(AudioManagerModule __instance, CycleController cycleController)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.ParticalDisruptor)
                return true;

            // Todo: Test
            MicroHIDData data = customItem.CustomItem.CustomData as MicroHIDData;
            if (data.MuteChargeAudio && cycleController.Phase is MicroHidPhase.WindingUp)
                return false;
            if (data.MuteFiringAudio && cycleController.Phase is MicroHidPhase.Firing)
                return false;
                
            return true;
        }
    }
}