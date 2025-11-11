using HarmonyLib;
using InventorySystem.Items.Scp1509;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches.SCP1509
{
    [HarmonyPatch(typeof(Scp1509Item))]
    public static class HumeShield
    {
        [HarmonyPatch(nameof(Scp1509Item.HsMax), MethodType.Getter)]
        [HarmonyPostfix]
        public static void MaxPostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.HumeshieldMax;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.HumeshieldMax;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.HsRegeneration), MethodType.Getter)]
        [HarmonyPostfix]
        public static void RegenPostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.HumeshieldRegeneration;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.HumeshieldRegeneration;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.ShieldRegenRate), MethodType.Getter)]
        [HarmonyPostfix]
        public static void RegenRatePostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.HumeshieldRegenRate;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.HumeshieldRegenRate;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.ShieldDecayRate), MethodType.Getter)]
        [HarmonyPostfix]
        public static void DecayRatePostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.HumeshieldDecayRate;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.HumeshieldDecayRate;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.ShieldOnDamagePause), MethodType.Getter)]
        [HarmonyPostfix]
        public static void DamagepausePostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.HumeshieldOnDamagePauseTime;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.HumeshieldOnDamagePauseTime;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.UnequipDecayDelay), MethodType.Getter)]
        [HarmonyPostfix]
        public static void DecayDelayPostfix(Scp1509Item __instance, ref float __result)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __result = data.UnequipHumeshieldDecayDelay;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.UnequipHumeshieldDecayDelay;
            }
        }
    }
}