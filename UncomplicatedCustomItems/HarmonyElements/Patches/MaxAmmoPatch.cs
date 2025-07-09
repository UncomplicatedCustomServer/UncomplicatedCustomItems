using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(MagazineModule), nameof(MagazineModule.AmmoMax), MethodType.Getter)]
    internal static class MaxAmmoPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MagazineModule __instance, ref int __result)
        {
            if (API.Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
            {
                WeaponData weaponData = customItem.CustomItem.CustomData as WeaponData;
                __result = weaponData.MaxMagazineAmmo;
                __instance.ServerResyncData();
                return false;
            }
            return true;
        }
    }
}