using System;
using InventorySystem;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Extensions
{
    public static class CustomItemExtensions
    {
        /// <summary>
        /// Adds the specified <see cref="CustomFlags"/> to the specified <see cref="SummonedCustomItem"/>.
        /// </summary>
        /// <param name="customitem"></param>
        /// <param name="flag"></param>
        public static void AddCustomFlag(this SummonedCustomItem customitem, string flag)
        {
            if (customitem?.CustomItem == null || string.IsNullOrWhiteSpace(flag))
                return;

            if (Enum.TryParse(flag, ignoreCase: true, out CustomFlags customflag))
            {
                if (customitem.CustomItem.CustomFlags == null)
                    customitem.CustomItem.CustomFlags = customflag;
                else
                    customitem.CustomItem.CustomFlags |= customflag;
            }
        }

        /// <summary>
        /// Removes the specified <see cref="CustomFlags"/> from the specified <see cref="SummonedCustomItem"/>.
        /// </summary>
        /// <param name="customitem"></param>
        /// <param name="flag"></param>
        public static void RemoveCustomFlag(this SummonedCustomItem customitem, string flag)
        {
            if (customitem?.CustomItem == null || string.IsNullOrWhiteSpace(flag))
                return;

            if (Enum.TryParse(flag, ignoreCase: true, out CustomFlags customflag))
            {
                if (customitem.HasModule(customflag))
                    customitem.CustomItem.CustomFlags &= ~customflag;
            }
        }

        /// <summary>
        /// Adds the specified <see cref="AttachmentName"/> to the specified <see cref="SummonedCustomItem"/>.
        /// </summary>
        /// <param name="customitem"></param>
        /// <param name="attachment"></param>
        /// <param name="pickup"></param>
        public static void AddAttachment(this SummonedCustomItem customitem, string attachment, bool pickup = false)
        {
            if (Enum.TryParse(attachment, ignoreCase: true, out AttachmentName attachmentname))
            {
                if (pickup)
                {
                    FirearmPickup firearm = customitem.Pickup as FirearmPickup;
                    if (firearm.Base.TryApplyAttachment(attachmentname))
                        LogManager.Debug($"Added {attachmentname} to {customitem.CustomItem.Name}");
                    else
                        LogManager.Error($"Failed to add {attachmentname} to {customitem.CustomItem.Name}");
                }
                else
                {
                    FirearmItem firearm = customitem.Item as FirearmItem;
                    if (firearm.Base.TryApplyAttachment(attachmentname))
                        LogManager.Debug($"Added {attachmentname} to {customitem.CustomItem.Name}");
                    else
                        LogManager.Error($"Failed to add {attachmentname} to {customitem.CustomItem.Name}");
                }
            }
            else
                LogManager.Warn($"Invalid attachment name: {attachment}");
        }

    }
}