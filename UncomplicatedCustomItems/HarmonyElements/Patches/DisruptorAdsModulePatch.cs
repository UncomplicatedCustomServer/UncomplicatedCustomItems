
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    // Neither are synced with client but still are applied.
    [HarmonyPatch(typeof(DisruptorAdsModule), nameof(DisruptorAdsModule.BaseAdsInaccuracy), MethodType.Getter)]
    public static class DisruptorAdsModuleAimingInaccuracyPatch
    {
        public static bool Prefix(DisruptorAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.ParticleDisruptor)
                return true;

            IParticleDisruptorData data = customItem.CustomItem.CustomData as IParticleDisruptorData;
            __result = data.AimingInaccuracy;
            return false;
        }
    }

    [HarmonyPatch(typeof(DisruptorAdsModule), nameof(DisruptorAdsModule.BaseHipInaccuracy), MethodType.Getter)]
    public static class DisruptorAdsModuleHipInaccuracyPatch
    {
        public static bool Prefix(DisruptorAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.ParticleDisruptor)
                return true;

            IParticleDisruptorData data = customItem.CustomItem.CustomData as IParticleDisruptorData;
            __result = data.Inaccuracy;
            return false;
        }
    }
}