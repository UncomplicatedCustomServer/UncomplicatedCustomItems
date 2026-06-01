using System;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class CustomItemExtensions
    {
        /// <summary>
        /// Adds the specified <see cref="AttachmentName"/> to the specified <see cref="SummonedCustomItem"/>.
        /// </summary>
        /// <param name="customitem"></param>
        /// <param name="attachment"></param>
        /// <param name="pickup"></param>
        public static void AddAttachment(this SummonedCustomItem customitem, string attachment, bool pickup = false)
        {
            if (Enum.TryParse(attachment, ignoreCase: true, out AttachmentName attachmentname) && customitem.CustomItem.Item.IsWeapon())
            {
                if (pickup)
                {
                    if (customitem.Pickup is not FirearmPickup firearm)
                        return;

                    if (firearm.Base.TryApplyAttachment(attachmentname))
                    {
                        LogManager.Debug($"Added {attachmentname} to {customitem.CustomItem.Name}");
                    }
                    else
                        LogManager.Error($"Failed to add {attachmentname} to {customitem.CustomItem.Name}");
                }
                else
                {
                    if (customitem.Item is not FirearmItem firearm)
                        return;

                    if (firearm.Base.TryApplyAttachment(attachmentname))
                    {
                        LogManager.Debug($"Added {attachmentname} to {customitem.CustomItem.Name}");
                    }
                    else
                        LogManager.Error($"Failed to add {attachmentname} to {customitem.CustomItem.Name}");
                }
            }
            else
                LogManager.Warn($"Invalid attachment name: {attachment}");
        }

    }
}