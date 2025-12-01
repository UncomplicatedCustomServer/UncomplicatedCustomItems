using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
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
            if (Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var item) && item.CustomItem.CustomData is WeaponData wd)
                __result = wd.Inaccuracy;

            if (SummonedAPICustomItem.TryGet(__instance.Firearm.ItemSerial, out var api) && api.CustomItem is CustomWeapon cw)
                __result = cw.Inaccuracy;
        }

        [HarmonyPatch("EffectiveAdsInaccuracy", MethodType.Getter)]
        [HarmonyPostfix]
        public static void EffectiveAdsInaccuracyPostfix(LinearAdsModule __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var item) && item.CustomItem.CustomData is WeaponData wd)
                __result = wd.AimingInaccuracy;

            if (SummonedAPICustomItem.TryGet(__instance.Firearm.ItemSerial, out var api) && api.CustomItem is CustomWeapon cw)
                __result = cw.AimingInaccuracy;
        }
    }
}