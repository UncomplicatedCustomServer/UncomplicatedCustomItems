using HarmonyLib;
using InventorySystem.Items.Jailbird;
using Mirror;
using UncomplicatedCustomItems.Extensions;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
    public static class JailbirdItemChargePatch
    {
        public static bool Prefix(JailbirdItem __instance, NetworkReader reader)
        {
            reader.ReadByte();
            reader.Position -= 1;
            JailbirdMessageType messageType = (JailbirdMessageType)reader.ReadByte();
            reader.Position -= 1;

            if (messageType == JailbirdMessageType.ChargeLoadTriggered || messageType == JailbirdMessageType.ChargeStarted)
            {
                if (API.Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var customItem))
                {
                    if (customItem.Item.Type == ItemType.Jailbird && customItem.HasModule(Enums.CustomFlags.NoCharge))
                    {
                        __instance.SendRpc(JailbirdMessageType.ChargeFailed);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
