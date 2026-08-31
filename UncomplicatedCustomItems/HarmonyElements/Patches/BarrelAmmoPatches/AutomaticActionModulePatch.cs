using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AutomaticActionModule))]
    internal static class AutomaticActionModulePatch
    {
        [HarmonyPatch("get_ChamberSize")]
        [HarmonyPrefix]
        public static bool Prefix(AutomaticActionModule __instance, ref int __result)
        {
            string? itemName = null;
            uint? itemId = null;
            int maxBarrelAmmo;

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem) && summonItem?.CustomItem is CustomWeapon customWeapon)
            {
                maxBarrelAmmo = customWeapon.MaxBarrelAmmo;
                itemName = customWeapon.Name;
                itemId = customWeapon.Id;
            }
            else if (__instance.Firearm != null && Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) && customItem?.CustomItem.CustomData is WeaponData data)
            {
                maxBarrelAmmo = data.MaxBarrelAmmo;
                itemName = customItem.CustomItem.Name;
                itemId = customItem.CustomItem.Id;
            }
            else
                return true;

            if (maxBarrelAmmo > 16)
            {
                maxBarrelAmmo = 16;
                LogManager.Warn($"Max Barrel Ammo for CustomItem {itemName} - {itemId} is greater than 16! The max value for Max Barrel Ammo is 16!");
            }

            __result = maxBarrelAmmo;
            __instance.ServerResync();
            return false;
        }
    }
}