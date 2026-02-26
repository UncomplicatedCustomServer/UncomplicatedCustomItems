using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
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
                case FirearmDamageHandler firearmDamage:
                    if (Utilities.TryGetSummonedCustomItem(firearmDamage.Firearm.ItemSerial, out var item) && item.CustomItem.CustomItemType is CustomItemType.Weapon && item.CustomItem.CustomData is WeaponData weaponData)
                    {
                        __result = weaponData.Damage;
                    }
                    if (SummonedAPICustomItem.TryGet(firearmDamage.Firearm.ItemSerial, out var apiitem) && apiitem.CustomItem is CustomWeapon customWeapon)
                    {
                        __result = customWeapon.Damage;
                    }
                    break;

                case MicroHidDamageHandler microHidDamage:
                    if (Utilities.TryGetSummonedCustomItem(Player.Get(microHidDamage.Attacker.Hub).CurrentItem.Serial, out var item1) && item1.CustomItem.CustomItemType is CustomItemType.MicroHID && item1.CustomItem.CustomData is MicroHIDData microData)
                    {
                        __result = microData.Damage;
                    }
                    break;

                case DisruptorDamageHandler disruptorDamage:
                    if (Utilities.TryGetSummonedCustomItem(Player.Get(disruptorDamage.Attacker.Hub).CurrentItem.Serial, out var item2) && item2.CustomItem.CustomItemType is CustomItemType.ParticleDisruptor && item2.CustomItem.CustomData is ParticleDisruptorData disruptorData)
                    {
                        __result = disruptorDamage.FiringState is FiringState.FiringRapid ? disruptorData.BurstDamage : disruptorData.ChargeDamage;
                    }
                    break;
            }
        }
    }
}