using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AutomaticActionModule), "get_ChamberSize")]
    internal static class AutomaticActionModulePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(AutomaticActionModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return true;

            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon)
                return true;

            IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;
            __result = Mathf.Clamp(weaponData.MaxBarrelAmmo, 0, 16);

            return false;
        }
    }
}