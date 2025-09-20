using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(PumpActionModule), "ShotsPerTriggerPull", MethodType.Getter)]
    public static class PumpActionModulePatch
    {
        public static bool Prefix(PumpActionModule __instance, ref int __result)
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