using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(CylinderAmmoModule), nameof(CylinderAmmoModule.AmmoMax), MethodType.Getter)]
    internal static class RevolverCylinderPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CylinderAmmoModule __instance, ref int __result)
        {
            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem) && summonItem.CustomItem is CustomWeapon cw)
            {
                __result = cw.MaxMagazineAmmo;
                __instance.ServerResync();
            }

            if (Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) && customItem.CustomItem.CustomData is WeaponData wd)
            {
                __result = wd.MaxMagazineAmmo;
                __instance.ServerResync();
            }
        }
    }
}