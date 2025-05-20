using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.Extensions
{
    public static class FirearmItemExtensions
    {
        public static bool IsAiming(this FirearmItem firearm) => firearm.Base.TryGetModule(out IAdsModule module) && module.AdsTarget;
        public static bool FlashLightStatus(this FirearmItem firearm) => firearm.Base.TryGetModule(out FlashlightAttachment module) && module.IsEnabled;
        
        public static Attachment GetAttachmentByName(this Firearm firearm, AttachmentName name)
        {
            foreach (Attachment attachment in firearm.Attachments)
            {
                if (attachment.Name == name)
                    return attachment;
            }

            return null;
        }
        
        public static bool TryApplyAttachment(this Firearm firearm, AttachmentName name)
        {
            Attachment targetAttachment = firearm.GetAttachmentByName(name);
            if (targetAttachment == null)
                return false;

            uint currentCode = firearm.GetCurrentAttachmentsCode();
            uint newCode = currentCode;

            for (int i = 0; i < firearm.Attachments.Length; i++)
            {
                Attachment attachment = firearm.Attachments[i];
                if (attachment.Slot == targetAttachment.Slot)
                {
                    uint bitToRemove = 1u << i;
                    newCode &= ~bitToRemove;
                }
            }

            for (int i = 0; i < firearm.Attachments.Length; i++)
            {
                if (firearm.Attachments[i] == targetAttachment)
                {
                    uint bitToAdd = 1u << i;
                    newCode |= bitToAdd;
                    break;
                }
            }

            firearm.ApplyAttachmentsCode(newCode, true);

            if (Mirror.NetworkServer.active)
            {
                firearm.ServerResendAttachmentCode();
            }

            return true;
        }

        public static bool TryApplyAttachment(this InventorySystem.Items.Firearms.FirearmPickup firearmPickup, AttachmentName name)
        {
            firearmPickup.Info.ItemId.TryGetTemplate<Firearm>(out var firearm);
            if (firearm == null)
                return false;

            AttachmentCodeSync.ServerSetCode(firearmPickup.Info.Serial, AttachmentsUtils.GetRandomAttachmentsCode(firearmPickup.Info.ItemId));
            bool success = TryApplyAttachment(firearm, name);
            if (success && firearm.WorldModel != null)
            {
                firearm.WorldModel.Setup(firearm.ItemId, firearm.WorldModel.WorldmodelType, firearm.GetCurrentAttachmentsCode());
                return true;
            }
            else
                return false;
        }
    }
}
