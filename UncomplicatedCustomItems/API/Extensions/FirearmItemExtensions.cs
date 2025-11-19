using InventorySystem;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using FirearmPickup = InventorySystem.Items.Firearms.FirearmPickup;
using static InventorySystem.Items.Firearms.Modules.AnimatorReloaderModuleBase;
using static InventorySystem.Items.Firearms.Modules.AutomaticActionModule;
using static InventorySystem.Items.Firearms.Modules.PumpActionModule;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class FirearmItemExtensions
    {
        public static bool IsAiming(this FirearmItem firearm) => firearm.Base.TryGetModule(out IAdsModule module) && module.AdsTarget;
        
        /// <summary>
        /// Extension version of <see cref="FirearmItem.GetCodeFromAttachmentNamesRaw"/>
        /// </summary>
        public static uint GetCodeFromAttachmentNamesRaw(this Firearm firearm, AttachmentName[] attachments)
        {
            uint attachmentNamesRaw = 0;
            uint num = 1;
            foreach (Attachment attachment in firearm.Attachments)
            {
                if (attachments.Contains(attachment.Name))
                    attachmentNamesRaw += num;
                num *= 2U;
            }

            return attachmentNamesRaw;
        }

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

            uint newCode = firearm.GetCurrentAttachmentsCode();

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
            firearm.ServerResendAttachmentCode();

            return true;
        }

        public static bool TryApplyAttachment(this FirearmPickup firearmPickup, AttachmentName name)
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

        public static bool TryTriggerAutomaticFakeShot(this FirearmItem item, int chambersFired = 1, bool dryFire = false)
        {
            if (!item.Base.TryGetModule<AutomaticActionModule>(out var actionModule))
                return false;

            if (dryFire)
            {
                actionModule.SendRpc(item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(MessageHeader.RpcDryFire);
                });

                actionModule.SendRpc(other => other != item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(MessageHeader.RpcDryFire);
                });
            }
            else
            {
                actionModule.SendRpc(item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(MessageHeader.RpcFire);
                    writer.WriteByte((byte)chambersFired);
                });

                actionModule.SendRpc(other => other != item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(MessageHeader.RpcFire);
                    writer.WriteByte((byte)chambersFired);
                });
            }

            return true;
        }

        public static bool TryTriggerRevolverFakeShot(this FirearmItem item, int chambersFired = 1, bool dryFire = false)
        {
            if (!item.Base.TryGetModule<DoubleActionModule>(out var actionModule))
                return false;

            if (dryFire)
            {
                actionModule.SendRpc(item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(DoubleActionModule.MessageType.RpcDryFire);
                });

                actionModule.SendRpc(other => other != item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(DoubleActionModule.MessageType.RpcDryFire);
                });
            }
            else
            {
                actionModule.SendRpc(item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(DoubleActionModule.MessageType.RpcFire);
                    writer.WriteByte((byte)chambersFired);
                });

                actionModule.SendRpc(other => other != item.CurrentOwner.ReferenceHub, writer =>
                {
                    writer.WriteSubheader(DoubleActionModule.MessageType.RpcFire);
                    writer.WriteByte((byte)chambersFired);
                });
            }

            return true;
        }

        public static bool TryTriggerPumpFakeShot(this FirearmItem item, int chambersFired = 1)
        {
            if (!item.Base.TryGetModule<PumpActionModule>(out var actionModule))
                return false;

            actionModule.SendRpc(item.CurrentOwner.ReferenceHub, writer =>
            {
                writer.WriteSubheader(RpcType.Shoot);
                writer.WriteByte((byte)chambersFired);
            });

            actionModule.SendRpc(other => other != item.CurrentOwner.ReferenceHub, writer =>
            {
                writer.WriteSubheader(RpcType.Shoot);
                writer.WriteByte((byte)chambersFired);
            });

            return true;
        }

        public static bool TryTriggerDisruptorFakeShot(this FirearmItem item, int chambersFired = 1)
        {
            if (!item.Base.TryGetModule<DisruptorActionModule>(out var actionModule))
                return false;

            actionModule.SendRpc(item.CurrentOwner.ReferenceHub, writer =>
            {
                writer.WriteSubheader(DisruptorActionModule.MessageType.RpcStartFiring);
                writer.WriteByte((byte)chambersFired);
            });

            actionModule.SendRpc(other => other != item.CurrentOwner.ReferenceHub, writer =>
            {
                writer.WriteSubheader(DisruptorActionModule.MessageType.RpcStartFiring);
                writer.WriteByte((byte)chambersFired);
            });

            return true;
        }
    }
}
