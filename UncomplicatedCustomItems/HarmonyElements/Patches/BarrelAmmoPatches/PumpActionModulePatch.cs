using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Enums;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(PumpActionModule), "ShotsPerTriggerPull", MethodType.Getter)]
    public static class PumpActionShotsPerTriggerPatch
    {
        public static bool Prefix(PumpActionModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return true;

            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon)
                return true;

            IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;

            __result = weaponData.MaxBarrelAmmo;
            __instance.ServerResync();
            return false;
        }
    }
}