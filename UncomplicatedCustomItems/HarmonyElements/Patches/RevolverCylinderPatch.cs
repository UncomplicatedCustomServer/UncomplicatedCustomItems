using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    // TODO: Test this.
    [HarmonyPatch(typeof(CylinderAmmoModule), nameof(CylinderAmmoModule.AmmoMax), MethodType.Getter)]
    internal static class RevolverCylinderPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CylinderAmmoModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon)
                return true;
            
                WeaponData weaponData = customItem.CustomItem.CustomData as WeaponData;
                __result = weaponData.MaxMagazineAmmo;
                __instance.ServerResync();
                return false;
        }
    }
}