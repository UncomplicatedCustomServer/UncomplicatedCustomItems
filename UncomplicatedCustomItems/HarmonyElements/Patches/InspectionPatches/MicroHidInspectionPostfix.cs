using HarmonyLib;
using InventorySystem.Items.MicroHID.Modules;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents;
using UncomplicatedCustomItems.Events.Handlers;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(DrawAndInspectorModule), nameof(DrawAndInspectorModule.ServerProcessCmd))]
    public static class MicroHidInspectionPostfix
    {
        [HarmonyPrefix]
        public static void Prefix(DrawAndInspectorModule __instance)
        {
            InspectingItemEventArgs args = new(Item.Get(__instance.ItemSerial), Player.Get(__instance.MicroHid.Owner));
            ItemInspectionEvents.OnInspectingItem(args);
            if (!args.IsAllowed)
                return;

            Timing.CallDelayed(Timing.WaitForOneFrame, () => ItemInspectionEvents.OnInspectedItem(new InspectedItemEventArgs(Item.Get(__instance.ItemSerial), Player.Get(__instance.MicroHid.Owner))));

            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                return;

            customItem.HandleEvent(Player.Get(__instance.MicroHid.Owner), ItemEvents.Inspect, __instance.ItemSerial);
        }
    }
}