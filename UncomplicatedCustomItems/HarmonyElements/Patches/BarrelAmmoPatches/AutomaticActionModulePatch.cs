using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AutomaticActionModule), "get_ChamberSize")]
    internal static class AutomaticActionModulePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(AutomaticActionModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || !SummonedBaseCustomItem.TryGet(__instance.ItemSerial, out var summonItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon || summonItem.CustomItem is not CustomWeapon customWeapon)
                return true;

            if (customItem != null)
            {
                IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;
                __result = weaponData.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }
            else if (summonItem != null)
            {
                __result = customWeapon.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }

            return true;
        }
    }
}