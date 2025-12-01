using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using Mirror;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
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
            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem) && summonItem.CustomItem is CustomWeapon customWeapon)
            {
                if (customWeapon.MaxBarrelAmmo > 16)
                {
                    customWeapon.MaxBarrelAmmo = 16;
                    LogManager.Warn($"Max Barrel Ammo for CustomItem {customWeapon.Name} - {customWeapon.Id} is greater than 16! The max value for Max Barrel Ammo is 16!");
                }

                __result = customWeapon.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }

            if (Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) && customItem.CustomItem.CustomData is WeaponData data)
            {
                if (data.MaxBarrelAmmo > 16)
                {
                    data.MaxBarrelAmmo = 16;
                    LogManager.Warn($"Max Barrel Ammo for CustomItem {customItem.CustomItem.Name} - {customItem.CustomItem.Id} is greater than 16! The max value for Max Barrel Ammo is 16!");
                }

                __result = data.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }

            return true;
        }
    }
}