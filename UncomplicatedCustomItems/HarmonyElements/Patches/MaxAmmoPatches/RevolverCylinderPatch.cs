using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(CylinderAmmoModule), nameof(CylinderAmmoModule.AmmoMax), MethodType.Getter)]
    internal static class RevolverCylinderPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CylinderAmmoModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || !SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon || summonItem.CustomItem is not CustomWeapon customWeapon)
                return true;

            if (customItem != null)
            {
                IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;
                __result = weaponData.MaxMagazineAmmo;
                __instance.ServerResync();
                return false;
            }
            else if (summonItem != null)
            {
                __result = customWeapon.MaxMagazineAmmo;
                __instance.ServerResync();
            }

            return true;
        }
    }
}