using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(MagazineModule), nameof(MagazineModule.AmmoMax), MethodType.Getter)]
    internal static class MaxAmmoPatch
    {
        [HarmonyPostfix]
        public static void Postfix(MagazineModule __instance, ref int __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var summonedCustomItem) && summonedCustomItem.CustomItem.CustomData is WeaponData wd)
            {
                __result = wd.MaxMagazineAmmo;
                __instance.ServerResyncData();
            }

            if (SummonedAPICustomItem.TryGet(__instance.Firearm.ItemSerial, out var apiItem) && apiItem.CustomItem is CustomWeapon cw)
            {
                __result = cw.MaxMagazineAmmo;
                __instance.ServerResyncData();
            }
        }
    }
}