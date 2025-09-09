using HarmonyLib;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms;
using Mirror;
using UncomplicatedCustomItems.API.Extensions;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Enums;

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

            if (hub.inventory.CurInstance is not Firearm firearm)
                return true;

            if (API.Utilities.TryGetSummonedCustomItem(firearm.ItemSerial, out var customItem))
            {
                if (customItem.Item.Type.IsWeapon() && customItem.HasModule(CustomFlags.WorkstationBan))
                {
                    Player.TryGet(hub.gameObject, out Player player);
                    player.SendHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
                    return false;
                }
            }
            return true;
        }
    }
}
