using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using System;
using UnityEngine;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    [HarmonyPatch]
    internal static class HitscanHitregModuleBasePatch
    {
        public static event Action<Player, Item, DestructibleHitPair, HitscanResult>? OnDamageDestructible;

        public static event Action<Player, Item, Ray, HitscanResult>? OnAppendPrescan;

        [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyDestructibleDamage))]
        [HarmonyPostfix]
        public static void ApplyDestructibleDamagePostfix(HitscanHitregModuleBase __instance, DestructibleHitPair target, HitscanResult result)
        {
            OnDamageDestructible?.Invoke(Player.Get(__instance.Owner), Item.Get(__instance.Item), target, result);
        }

        [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerAppendPrescan))]
        [HarmonyPostfix]
        public static void AppendPrescanPostfix(HitscanHitregModuleBase __instance, Ray targetRay, HitscanResult toAppend)
        {
            OnAppendPrescan?.Invoke(Player.Get(__instance.Owner), Item.Get(__instance.Item), targetRay, toAppend);
        }
    }
}