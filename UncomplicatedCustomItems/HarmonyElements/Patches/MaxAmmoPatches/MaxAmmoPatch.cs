using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(MagazineModule), nameof(MagazineModule.AmmoMax), MethodType.Getter)]
    internal static class MaxAmmoPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MagazineModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var summonedCustomItem) || !SummonedBaseCustomItem.TryGet(__instance.ItemSerial, out var summonItem))
                return true;
            if (summonedCustomItem.CustomItem.CustomItemType is not CustomItemType.Weapon || summonItem.CustomItem is not CustomWeapon customWeapon)
                return true;

            if (summonedCustomItem != null)
            {
                IWeaponData weaponData = summonedCustomItem.CustomItem.CustomData as IWeaponData;
                __result = weaponData.MaxMagazineAmmo;
                __instance.ServerResyncData();
                return false;
            }
            else if (summonItem != null)
            {
                __result = customWeapon.MaxMagazineAmmo;
                __instance.ServerResyncData();
                return false;   
            }

            return true;
        }
    }
}