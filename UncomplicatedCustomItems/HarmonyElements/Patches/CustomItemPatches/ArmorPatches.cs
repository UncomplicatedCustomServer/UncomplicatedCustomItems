using System;
using HarmonyLib;
using InventorySystem.Items.Armor;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(BodyArmor), nameof(BodyArmor.StaminaRegenMultiplier), MethodType.Getter)]
    internal static class StaminaRegenMultiplierPatch
    {
        public static bool Prefix(ref float __result, BodyArmor __instance)
        {
            try
            {
                if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is ArmorData ad && !item.IsPickup)
                {
                    __result = __instance.ProcessMultiplier(ad.StaminaRegenMultiplier);
                    return false;
                }

                if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var api) && api.CustomItem is CustomArmor ca && !api.IsPickup)
                {
                    __result = __instance.ProcessMultiplier(ca.StaminaRegenMultiplier);
                    return false;
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