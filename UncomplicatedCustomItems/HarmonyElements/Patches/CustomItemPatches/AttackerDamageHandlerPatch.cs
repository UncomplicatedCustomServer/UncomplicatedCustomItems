using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.SpecificData;
using static InventorySystem.Items.Firearms.Modules.DisruptorActionModule;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    [HarmonyPatch]
    public static class AttackerDamageHandlerPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(FirearmDamageHandler), nameof(FirearmDamageHandler.Damage));
            yield return AccessTools.PropertyGetter(typeof(MicroHidDamageHandler), nameof(MicroHidDamageHandler.Damage));
            yield return AccessTools.PropertyGetter(typeof(DisruptorDamageHandler), nameof(DisruptorDamageHandler.Damage));
        }

        public static void Postfix(AttackerDamageHandler __instance, ref float __result)
        {
            switch (__instance)
            {
                case MicroHidDamageHandler microHidDamage when microHidDamage.Attacker.Hub != null:
                    Player? microPlayer = Player.Get(microHidDamage.Attacker.Hub);
                    ushort microSerial = microPlayer?.CurrentItem?.Serial ?? 0;
                    if (Utilities.TryGetSummonedCustomItem(microSerial, out var item1) && item1 != null && item1.CustomItem.CustomItemType is CustomItemType.MicroHID && item1.CustomItem.CustomData is MicroHIDData microData)
                        __result = microData.Damage;

                    break;

                case DisruptorDamageHandler disruptorDamage when disruptorDamage.Attacker.Hub != null:
                    Player? disruptorPlayer = Player.Get(disruptorDamage.Attacker.Hub);
                    ushort disruptorSerial = disruptorPlayer?.CurrentItem?.Serial ?? 0;
                    if (Utilities.TryGetSummonedCustomItem(disruptorSerial, out var item2) && item2 != null && item2.CustomItem.CustomItemType is CustomItemType.ParticleDisruptor && item2.CustomItem.CustomData is ParticleDisruptorData disruptorData)
                        __result = disruptorDamage.FiringState is FiringState.FiringRapid ? disruptorData.BurstDamage : disruptorData.ChargeDamage;

                    break;
            }
        }
    }
}