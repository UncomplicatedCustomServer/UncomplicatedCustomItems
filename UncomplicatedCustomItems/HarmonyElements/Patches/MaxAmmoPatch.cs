using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(MagazineModule), nameof(MagazineModule.AmmoMax), MethodType.Getter)]
    internal static class MaxAmmoPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MagazineModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return true;
                
            if (customItem.CustomItem.CustomItemType is CustomItemType.Weapon)
            {
                WeaponData weaponData = customItem.CustomItem.CustomData as WeaponData;
                __result = weaponData.MaxMagazineAmmo;
                __instance.ServerResyncData();
            }

            if (customItem.CustomItem.CustomItemType is CustomItemType.ParticalDisruptor)
            {
                ParticalDisruptorData weaponData = customItem.CustomItem.CustomData as ParticalDisruptorData;
                __result = weaponData.Ammo;
                __instance.ServerResyncData();
            }
                return false;
        }
    }
}