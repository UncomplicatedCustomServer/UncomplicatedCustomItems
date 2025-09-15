using System;
using HarmonyLib;
using InventorySystem.Items.Armor;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(BodyArmor), nameof(BodyArmor.StaminaRegenMultiplier), MethodType.Getter)]
    internal static class StaminaRegenMultiplierPatch
    {
        public static bool Prefix(ref float __result, BodyArmor __instance)
        {
            try
            {
                if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                    return true;
                if (customItem.CustomItem.CustomItemType is not CustomItemType.Armor)
                    return true;
                if (customItem.IsPickup)
                    return true;
                    
                IArmorData data = customItem.CustomItem.CustomData as IArmorData;
                __result = __instance.ProcessMultiplier(data.StaminaRegenMultiplier);
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(StaminaRegenMultiplierPatch)}: {ex.Message}\n{ex.StackTrace}");
            }

            return true;
        }
    }
}