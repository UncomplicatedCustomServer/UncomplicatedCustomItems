using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents;
using UncomplicatedCustomItems.Events.Handlers;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(SimpleInspectorModule), nameof(SimpleInspectorModule.ServerProcessCmd))]
    public static class WeaponInspectionPostfix
    {
        [HarmonyPrefix]
        public static void Prefix(SimpleInspectorModule __instance)
        {
            InspectingItemEventArgs args = new(Item.Get(__instance.ItemSerial), Player.Get(__instance.Firearm.Owner));
            ItemInspectionEvents.OnInspectingItem(args);
            if (!args.IsAllowed)
                return;

            Timing.CallDelayed(Timing.WaitForOneFrame, () => ItemInspectionEvents.OnInspectedItem(new InspectedItemEventArgs(Item.Get(__instance.ItemSerial), Player.Get(__instance.Firearm.Owner))));

            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem))
                return;

            customItem.HandleEvent(Player.Get(__instance.Firearm.Owner), ItemEvents.Inspect, __instance.Firearm.ItemSerial);
        }
    }
}