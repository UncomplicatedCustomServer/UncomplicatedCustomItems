using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using System;
using UnityEngine;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerAppendPrescan))]
    internal static class AppendPrescanPatch
    {
        public static event Action<Player, Item, Ray, HitscanResult>? OnAppendPrescan;

        public static void Postfix(HitscanHitregModuleBase __instance, Ray targetRay, HitscanResult toAppend)
        {
            OnAppendPrescan?.Invoke(Player.Get(__instance.Owner), Item.Get(__instance.Item), targetRay, toAppend);
        }
    }
}
