using HarmonyLib;
using InventorySystem.Items.Scp1509;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches.SCP1509
{
    [HarmonyPatch(typeof(Scp1509Item))]
    public static class Revive
    {
        [HarmonyPatch(nameof(Scp1509Item._reviveCooldown), MethodType.Getter)]
        [HarmonyPostfix]
        public static void ReviveCooldownPostfix(Scp1509Item __instance, ref double __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.ReviveCooldown;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.CanResurrect), MethodType.Getter)]
        [HarmonyPostfix]
        public static void CanResurrectPostfix(Scp1509Item __instance, ref bool __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.CanResurrect;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item._revivedPlayerAOEBonusAHP), MethodType.Getter)]
        [HarmonyPostfix]
        public static void DecayRatePostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.RevivedPlayeraoeBonusahp;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.RevivedPlayerMaxAHP), MethodType.Getter)]
        [HarmonyPostfix]
        public static void RevivedPlayerMaxAHPPostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.RevivedPlayerMaxahp;
            }
        }
    }
}