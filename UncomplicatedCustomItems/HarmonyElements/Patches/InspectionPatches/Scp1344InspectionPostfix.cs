using HarmonyLib;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using InventorySystem.Items.Usables.Scp1344;
using UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents;
using UncomplicatedCustomItems.Events.Handlers;
using MEC;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(Scp1344NetworkHandler), nameof(Scp1344NetworkHandler.TryInspect))]
    public static class Scp1344InspectionPostfix
    {
        [HarmonyPrefix]
        public static void Prefix(InventorySystem.Items.Usables.Scp1344.Scp1344Item __instance)
        {
            InspectingItemEventArgs args = new(Item.Get(__instance.ItemSerial), Player.Get(__instance.Owner));
            ItemInspectionEvents.OnInspectingItem(args);
            if (!args.IsAllowed)
                return;

            Timing.CallDelayed(Timing.WaitForOneFrame, () => ItemInspectionEvents.OnInspectedItem(new InspectedItemEventArgs(Item.Get(__instance.ItemSerial), Player.Get(__instance.Owner))));

            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                return;

            customItem.HandleEvent(Player.Get(__instance.Owner), ItemEvents.Inspect, __instance.ItemSerial);
        }
    }
}