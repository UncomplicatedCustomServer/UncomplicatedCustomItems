using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{

    [HarmonyPatch(typeof(LinearAdsModule))]
    internal static class LinearADSModulePostfix
    {
        [HarmonyPatch("EffectiveHipInaccuracy", MethodType.Getter)]
        [HarmonyPostfix]
        public static void EffectiveHipInaccuracyPostfix(LinearAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var item))
                return;

            if (item.CustomItem.CustomData is not WeaponData data)
                return;

            __result = data.Inaccuracy;
        }

        [HarmonyPatch("EffectiveAdsInaccuracy", MethodType.Getter)]
        [HarmonyPostfix]
        public static void EffectiveAdsInaccuracyPostfix(LinearAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var item))
                return;

            if (item.CustomItem.CustomData is not WeaponData data)
                return;

            __result = data.AimingInaccuracy;
        }
    }
}