using HarmonyLib;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms;
using Mirror;
using UncomplicatedCustomItems.Extensions;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AttachmentsServerHandler), nameof(AttachmentsServerHandler.ServerReceiveChangeRequest))]
    internal static class PreventAttachmentChangePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NetworkConnection conn, AttachmentsChangeRequest msg)
        {
            if (!ReferenceHub.TryGetHub(conn, out ReferenceHub hub))
                return true;

            Firearm firearm = hub.inventory.CurInstance as Firearm;
            if (firearm == null)
                return true;

            if (API.Utilities.TryGetSummonedCustomItem(firearm.ItemSerial, out var customItem))
            {
                if (customItem.Item.Type.IsWeapon() && customItem.HasModule(Enums.CustomFlags.WorkstationBan))
                {
                    Player.TryGet(hub.gameObject, out var player);
                    player.SendHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
                    return false;
                }
            }
            return true;
        }
    }
}
