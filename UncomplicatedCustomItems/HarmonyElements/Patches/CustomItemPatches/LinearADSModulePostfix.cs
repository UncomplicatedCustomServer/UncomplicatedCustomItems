using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
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
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || !SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem))
                return;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon || summonItem.CustomItem is not CustomWeapon customWeapon)
                return;

            if (customItem != null)
            {
                WeaponData data = customItem.CustomItem.CustomData as WeaponData;
                __result = data.Inaccuracy;
            }
            else if (summonItem != null)
                __result = customWeapon.Inaccuracy;
        }

        [HarmonyPatch("EffectiveAdsInaccuracy", MethodType.Getter)]
        [HarmonyPostfix]
        public static void EffectiveAdsInaccuracyPostfix(LinearAdsModule __instance, ref float __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || !SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem))
                return;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon || summonItem.CustomItem is not CustomWeapon customWeapon)
                return;

            if (customItem != null)
            {
                WeaponData data = customItem.CustomItem.CustomData as WeaponData;
                __result = data.AimingInaccuracy;
            }
            else if (summonItem != null)
                __result = customWeapon.AimingInaccuracy;
        }
    }
}