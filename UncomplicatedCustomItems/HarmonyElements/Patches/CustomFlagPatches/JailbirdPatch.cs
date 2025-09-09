using System;
using HarmonyLib;
using InventorySystem.Items.Jailbird;
using Mirror;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
    internal static class JailbirdItemChargePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(JailbirdItem __instance, NetworkReader reader)
        {
            try
            {
                JailbirdMessageType messageType = (JailbirdMessageType)reader.ReadByte();

                if (messageType == JailbirdMessageType.ChargeLoadTriggered || messageType == JailbirdMessageType.ChargeStarted)
                {
                    if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                    {
                        if (customItem.Item.Type == ItemType.Jailbird && customItem.HasModule(CustomFlags.NoCharge))
                        {
                            __instance.SendRpc(JailbirdMessageType.ChargeFailed);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(JailbirdItemChargePatch)}: {ex.Message}\n{ex.StackTrace}");
            }
            return true;
        }
    }
}
