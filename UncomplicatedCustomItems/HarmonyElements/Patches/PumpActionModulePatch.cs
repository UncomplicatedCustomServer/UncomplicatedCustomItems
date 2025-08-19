using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UnityEngine;
using System.Reflection;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Enums;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(PumpActionModule), "OnInit")]
    internal static class PumpActionModulePatch
    {
        [HarmonyPostfix]
        public static void Postfix(PumpActionModule __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return;

            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon)
                return;

            IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;

            FieldInfo field = typeof(PumpActionModule).GetField("_numberOfBarrels", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(__instance, Mathf.Max(weaponData.MaxBarrelAmmo, 1));
                __instance.ServerResync();
            }
        }
    }
}