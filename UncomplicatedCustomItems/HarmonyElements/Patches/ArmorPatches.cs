using System;
using HarmonyLib;
using InventorySystem.Items.Armor;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(BodyArmor), nameof(BodyArmor.StaminaRegenMultiplier), MethodType.Getter)]
    internal static class StaminaRegenMultiplierPatch
    {
        public static bool Prefix(ref float __result, BodyArmor __instance)
        {
            try
            {
                if (API.Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem) && customItem.CustomItem.CustomItemType == CustomItemType.Armor)
                {
                    if (customItem.Pickup == null)
                    {
                        IArmorData data = customItem.CustomItem.CustomData as IArmorData;
                        __result = __instance.ProcessMultiplier(data.StaminaRegenMultiplier);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(StaminaRegenMultiplierPatch)}: {ex.Message}\n{ex.StackTrace}");
            }

            return true;
        }
    }
}