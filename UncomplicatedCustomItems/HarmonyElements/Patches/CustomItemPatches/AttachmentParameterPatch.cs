using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using UncomplicatedCustomItems.API.Extensions;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AttachmentsUtils), nameof(AttachmentsUtils.AttachmentsValue))]
    public static class AttachmentParameterPatch
    {
        public static bool Prefix(Firearm firearm, AttachmentParam param, ref float __result)
        {
            if (firearm.IsSummonedCustomItem())
            {
                __result = 0f;
                return false;
            }

            return true;
        }
    }
}