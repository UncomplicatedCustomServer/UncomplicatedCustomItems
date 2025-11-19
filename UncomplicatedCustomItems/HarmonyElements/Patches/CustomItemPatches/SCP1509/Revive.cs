using HarmonyLib;
using InventorySystem.Items.Scp1509;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches.SCP1509
{
    [HarmonyPatch(typeof(Scp1509Item))]
    public static class Revive
    {
        [HarmonyPatch(nameof(Scp1509Item.ServerProcessKill))]
        [HarmonyPostfix]
        public static void ServerProcessKill_Postfix(Scp1509Item __instance, ReferenceHub ply)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                if (__instance._nextResurrectTime > NetworkTime.time)
                {
                    __instance._nextResurrectTime = NetworkTime.time + data.ReviveCooldown;
                }
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                if (__instance._nextResurrectTime > NetworkTime.time)
                {
                    __instance._nextResurrectTime = NetworkTime.time + customSCP1509.ReviveCooldown;
                }
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

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __result = customSCP1509.CanResurrect;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.ServerApplyResurrectEffects))]
        [HarmonyPrefix]
        public static void ServerApplyResurrectEffectsPrefix(Scp1509Item __instance, ReferenceHub victim, ReferenceHub resurrectedPlayer, RoleTypeId respawnRole)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                __instance._revivedPlayerAOEBonusAHP = data.RevivedPlayeraoeBonusahp;
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                __instance._revivedPlayerAOEBonusAHP = customSCP1509.RevivedPlayeraoeBonusahp;
            }
        }

        [HarmonyPatch(nameof(Scp1509Item.ServerApplyResurrectEffects))]
        [HarmonyPostfix]
        public static void ServerApplyResurrectEffectsPostfix(Scp1509Item __instance, ReferenceHub victim, ReferenceHub resurrectedPlayer, RoleTypeId respawnRole)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var item) && item.CustomItem.CustomData is SCP1509Data data)
            {
                AhpStat ahp = resurrectedPlayer.playerStats.GetModule<AhpStat>();
                ahp?.ServerAddProcess(data.RevivedPlayerMaxahp, data.RevivedPlayerMaxahp, 0f, 0.7f, 0f, false);
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomSCP1509 customSCP1509)
            {
                AhpStat ahp = resurrectedPlayer.playerStats.GetModule<AhpStat>();
                ahp?.ServerAddProcess(customSCP1509.RevivedPlayerMaxahp, customSCP1509.RevivedPlayerMaxahp, 0f, 0.7f, 0f, false);
            }
        }
    }
}