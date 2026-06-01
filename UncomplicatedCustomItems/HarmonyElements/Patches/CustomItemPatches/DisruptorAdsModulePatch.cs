
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    // Neither are synced with client but still are applied serverside.
    [HarmonyPatch(typeof(DisruptorAdsModule))]
    public static class DisruptorAdsModulePatch
    {
        [HarmonyPatch(nameof(DisruptorAdsModule.BaseAdsInaccuracy), MethodType.Getter)]
        public static bool AimingAccuracyPrefix(DisruptorAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || customItem == null)
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.ParticleDisruptor)
                return true;
            if (customItem.CustomItem.CustomData is not ParticleDisruptorData data)
                return true;

            __result = data.AimingInaccuracy;
            return false;
        }

        [HarmonyPatch(nameof(DisruptorAdsModule.BaseHipInaccuracy), MethodType.Getter)]
        public static bool HipAccuracyPrefix(DisruptorAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || customItem == null)
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.ParticleDisruptor)
                return true;
            if (customItem.CustomItem.CustomData is not ParticleDisruptorData data)
                return true;

            __result = data.Inaccuracy;
            return false;
        }
    }
}