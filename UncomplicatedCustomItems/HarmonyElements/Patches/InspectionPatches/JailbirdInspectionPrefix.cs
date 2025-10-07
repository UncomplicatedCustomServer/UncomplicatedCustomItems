using HarmonyLib;
using JailbirdItem = InventorySystem.Items.Jailbird.JailbirdItem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using InventorySystem.Items.Jailbird;
using Mirror;
using UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents;
using MEC;
using UncomplicatedCustomItems.Events.Handlers;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
    public static class JailbirdInspectionPrefix
    {
        [HarmonyPrefix]
        public static void Prefix(JailbirdItem __instance, NetworkReader reader)
        {
            reader.ReadByte();
            reader.Position -= 1;
            JailbirdMessageType messageType = (JailbirdMessageType)reader.ReadByte();
            reader.Position -= 1;

            if (messageType != JailbirdMessageType.Inspect)
                return;

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