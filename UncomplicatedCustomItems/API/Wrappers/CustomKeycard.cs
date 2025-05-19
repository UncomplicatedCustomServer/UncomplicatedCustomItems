using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Extensions;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Wrappers
{
    /// <summary>
    /// A wrapper class for creating and customizing keycards via UCI.
    /// Provides easy-to-use properties to set name tags, colors, permissions, and other metadata.
    /// </summary>
    /// <remarks>
    /// <para><b>If the keycard is an item:</b></para>
    /// <code>
    /// KeycardItem keycard = Item as KeycardItem;
    /// CustomKeycard customKeycard = new CustomKeycard(keycard.Base);
    /// KeycardUtils.RemoveKeycardDetail(keycard.Serial);
    /// KeycardDetailSynchronizer.ServerProcessItem(keycard.Base);
    /// </code>
    ///
    /// <para><b>If the keycard is a pickup:</b></para>
    /// <code>
    /// KeycardPickup keycard = (KeycardPickup)KeycardPickup.Create(CustomItem.Item, Pickup.Position);
    /// keycard.Base.Info.ItemId.TryGetTemplate&lt;KeycardItem&gt;(out KeycardItem item);
    /// item.ItemSerial = keycard.Serial;
    /// CustomKeycard customKeycard = new CustomKeycard(item);
    /// KeycardUtils.RemoveKeycardDetail(keycard.Serial);
    /// KeycardDetailSynchronizer.ServerProcessPickup(keycard.Base);
    /// </code>
    /// </remarks>
    public class CustomKeycard
    {
        private Dictionary<ushort, Color32> PermissionColorsDic = [];
        private Dictionary<ushort, Color32> LabelColorsDic = [];
        private Dictionary<ushort, Color32> KeycardColorsDic = [];
        private Dictionary<ushort, string> NameTagDic = [];
        private Dictionary<ushort, string> ItemNameDic = [];
        private Dictionary<ushort, string> LabelTextDic = [];
        private Dictionary<ushort, KeycardLevels> PermissionsDic = [];
        private Dictionary<ushort, string> SerialNumberDic = [];
        private Dictionary<ushort, byte> WearIndexDic = [];
        private Dictionary<ushort, int> RankIndexDic = [];

        /// <summary>
        /// The underlying Exiled <see cref="KeycardItem"/> instance being wrapped.
        /// </summary>
        public KeycardItem ParentKeycard { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomKeycard"/> class.
        /// </summary>
        /// <param name="keycard"> The <see cref="KeycardItem"/> to wrap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CustomKeycard(KeycardItem keycard)
        {
            if (!keycard.Customizable)
                LogManager.Warn($"{keycard.ItemTypeId} is not customizable!\nThe keycard type must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
            else
                ParentKeycard = keycard ?? throw new ArgumentNullException(nameof(keycard));
        }

        /// <summary>
        /// Gets or sets the name text shown on the <see cref="KeycardItem"/>.
        /// </summary>
        public string NameTag
        {
            get
            {
                NameTagDic.TryGetValue(ParentKeycard.ItemSerial, out string value);
                return value;
            }
            set
            {
                NametagDetail nametagDetail = ParentKeycard.Details.OfType<NametagDetail>().FirstOrDefault();
                if (nametagDetail != null)
                {
                    try
                    {
                        object[] args = { value.Replace("%name%", ParentKeycard.Owner.nicknameSync.MyNick) };
                        ArraySegment<object> arguments = new(args);
                        nametagDetail.SetArguments(arguments);
                        NameTagDic.TryAdd(ParentKeycard.ItemSerial, value);
                        if (Utilities.TryGetSummonedCustomItem(ParentKeycard.ItemSerial, out SummonedCustomItem summonedCustomItem))
                            summonedCustomItem.NameApplied = true;
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"Error processing CustomItemNameDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
                else
                    LogManager.Error($"{nameof(CustomKeycard)}: This keycard {ParentKeycard.ItemTypeId} doesn't have a NameTag section.");
            }
        }

        /// <summary>
        /// Gets or sets the background tint color of the <see cref="KeycardItem"/>.
        /// </summary>
        public Color32 CardColor
        {
            get
            {
                KeycardColorsDic.TryGetValue(ParentKeycard.ItemSerial, out Color32 value);
                return value;
            }
            set
            {
                CustomTintDetail tintDetail = ParentKeycard.Details.OfType<CustomTintDetail>().FirstOrDefault();
                if (tintDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        tintDetail.SetArguments(arguments);
                        KeycardColorsDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomTintDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the display name of the <see cref="KeycardItem"/> in inventory.
        /// </summary>
        public string ItemName
        {
            get
            {
                ItemNameDic.TryGetValue(ParentKeycard.ItemSerial, out string value);
                return value;
            }
            set
            {
                CustomItemNameDetail nameDetail = ParentKeycard.Details.OfType<CustomItemNameDetail>().FirstOrDefault();
                if (nameDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        nameDetail.SetArguments(arguments);
                        ItemNameDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomItemNameDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the color of the label text printed on the <see cref="KeycardItem"/>.
        /// Set this first before setting <see cref="LabelText"/>, or the label will not render.
        /// </summary>
        public Color32 LabelColor
        {
            get
            {
                LabelColorsDic.TryGetValue(ParentKeycard.ItemSerial, out Color32 value);
                return value;
            }
            set
            {
                CustomLabelDetail labelDetail = ParentKeycard.Details.OfType<CustomLabelDetail>().FirstOrDefault();
                if (labelDetail != null)
                {
                    try
                    {
                        LabelColorsDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomLabelDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the text printed on the label of the <see cref="KeycardItem"/>.
        /// </summary>
        public string LabelText
        {
            get
            {
                LabelTextDic.TryGetValue(ParentKeycard.ItemSerial, out string value);
                return value;
            }
            set
            {
                CustomLabelDetail labelDetail = ParentKeycard.Details.OfType<CustomLabelDetail>().FirstOrDefault();
                if (labelDetail != null)
                {
                    try
                    {
                        object[] args = { value, LabelColor };
                        ArraySegment<object> arguments = new(args);
                        labelDetail.SetArguments(arguments);
                        LabelTextDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomLabelDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the color used when rendering permissions on the <see cref="KeycardItem"/>.
        /// Set this first before setting <see cref="Permissions"/>, or the color will not render.
        /// </summary>
        public Color32 PermissionsColor
        {
            get
            {
                PermissionColorsDic.TryGetValue(ParentKeycard.ItemSerial, out Color32 value);
                return value;
            }
            set
            {
                PermissionColorsDic.TryAdd(ParentKeycard.ItemSerial, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="KeycardLevels"/> granted by this <see cref="KeycardItem"/>.
        /// </summary>
        public KeycardLevels Permissions
        {
            get
            {
                PermissionsDic.TryGetValue(ParentKeycard.ItemSerial, out KeycardLevels value);
                return value;
            }
            set
            {
                CustomPermsDetail permsDetail = ParentKeycard.Details.OfType<CustomPermsDetail>().FirstOrDefault();
                if (permsDetail != null)
                {
                    try
                    {
                        object[] args = { value, PermissionsColor };
                        ArraySegment<object> arguments = new(args);
                        permsDetail.SetArguments(arguments);
                        PermissionsDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomPermsDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a custom serial number string.
        /// Only for Task Force or Metal Case cards.
        /// </summary>
        public string SerialNumber
        {
            get
            {
                SerialNumberDic.TryGetValue(ParentKeycard.ItemSerial, out string value);
                return value;
            }
            set
            {
                CustomSerialNumberDetail serialNumberDetail = ParentKeycard.Details.OfType<CustomSerialNumberDetail>().FirstOrDefault();
                if (serialNumberDetail != null && (ParentKeycard.ItemTypeId == ItemType.KeycardCustomTaskForce || ParentKeycard.ItemTypeId == ItemType.KeycardCustomMetalCase))
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        serialNumberDetail.SetArguments(arguments);
                        SerialNumberDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomSerialNumberDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
                else
                    LogManager.Info("Custom Serial Numbers are only available on CustomTaskForce and CustomMetalCase keycards");
            }
        }

        /// <summary>
        /// Gets or sets the wear index.
        /// I dont think this is applied currently in the 14.1 beta.
        /// </summary>
        public byte WearIndex
        {
            get
            {
                WearIndexDic.TryGetValue(ParentKeycard.ItemSerial, out byte value);
                return value;
            }
            set
            {
                CustomWearDetail wearDetail = ParentKeycard.Details.OfType<CustomWearDetail>().FirstOrDefault();
                if (wearDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        wearDetail.SetArguments(arguments);
                        WearIndexDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomWearDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the rank shown on the card.
        /// I dont think this is applied currently in the 14.1 beta.
        /// </summary>
        public int RankIndex
        {
            get
            {
                RankIndexDic.TryGetValue(ParentKeycard.ItemSerial, out int value);
                return value;
            }
            set
            {
                CustomRankDetail rankDetail = ParentKeycard.Details.OfType<CustomRankDetail>().FirstOrDefault();
                if (rankDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        rankDetail.SetArguments(arguments);
                        RankIndexDic.TryAdd(ParentKeycard.ItemSerial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomRankDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }
    }
}
