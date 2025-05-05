using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
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
    /// A wrapper class for creating and customizing custom keycards via UCI.
    /// Provides easy-to-use properties to set name tags, colors, permissions, and other metadata.
    /// <para>Example usage:</para>
    /// <code>
    /// Keycard keycard = Item as Keycard;
    /// CustomKeycard customKeycard = new CustomKeycard(keycard);
    /// </code>
    /// </summary>
    public class CustomKeycard : IWrapper<KeycardItem>
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
        /// The underlying Exiled <see cref="Keycard"/> instance being wrapped.
        /// </summary>
        public Keycard ParentKeycard { get; private set; }

        /// <summary>
        /// The base <see cref="KeycardItem"/> object that this wrapper manages.
        /// </summary>
        public new KeycardItem Base { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomKeycard"/> class.
        /// </summary>
        /// <param name="keycard"> The <see cref="Keycard"/> to wrap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CustomKeycard(Keycard keycard)
        {
            ParentKeycard = keycard ?? throw new ArgumentNullException(nameof(keycard));
            if (!ParentKeycard.Base.Customizable)
                LogManager.Warn($"{ParentKeycard.Type} is not customizable!\nThe item must be 'KeycardCustomMetalCase', 'KeycardCustomManagement', 'KeycardCustomSite02', or 'KeycardCustomTaskForce'!");
        }
        /// <summary>
        /// Gets or sets the name text shown on the <see cref="Keycard"/>.
        /// </summary>
        public string NameTag
        {
            get
            {
                NameTagDic.TryGetValue(ParentKeycard.Serial, out string value);
                return value;
            }
            set
            {
                NametagDetail nametagDetail = ParentKeycard.Base.Details.OfType<NametagDetail>().FirstOrDefault();
                if (nametagDetail != null)
                {
                    try
                    {
                        object[] args = { value.Replace("%name%", ParentKeycard.Owner.DisplayNickname) };
                        ArraySegment<object> arguments = new(args);
                        nametagDetail.SetArguments(arguments);
                        NameTagDic.TryAdd(ParentKeycard.Serial, value);
                        if (Utilities.TryGetSummonedCustomItem(ParentKeycard.Serial, out SummonedCustomItem summonedCustomItem))
                            summonedCustomItem.NameApplied = true;
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"Error processing CustomItemNameDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
                else
                    LogManager.Error($"{nameof(CustomKeycard)}: This keycard {ParentKeycard.Type} doesn't have a NameTag section.");
            }
        }

        /// <summary>
        /// Gets or sets the background tint color of the <see cref="Keycard"/>.
        /// </summary>
        public Color32 CardColor
        {
            get
            {
                KeycardColorsDic.TryGetValue(ParentKeycard.Serial, out Color32 value);
                return value;
            }
            set
            {
                CustomTintDetail tintDetail = ParentKeycard.Base.Details.OfType<CustomTintDetail>().FirstOrDefault();
                if (tintDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        tintDetail.SetArguments(arguments);
                        KeycardColorsDic.TryAdd(ParentKeycard.Serial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomTintDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the display name of the <see cref="Keycard"/> in inventory.
        /// </summary>
        public string ItemName
        {
            get
            {
                ItemNameDic.TryGetValue(ParentKeycard.Serial, out string value);
                return value;
            }
            set
            {
                CustomItemNameDetail nameDetail = ParentKeycard.Base.Details.OfType<CustomItemNameDetail>().FirstOrDefault();
                if (nameDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        nameDetail.SetArguments(arguments);
                        ItemNameDic.TryAdd(ParentKeycard.Serial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomItemNameDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the color of the label text printed on the <see cref="Keycard"/>.
        /// Set this first before setting <see cref="LabelText"/>, or the label will not render.
        /// </summary>
        public Color32 LabelColor
        {
            get
            {
                LabelColorsDic.TryGetValue(ParentKeycard.Serial, out Color32 value);
                return value;
            }
            set
            {
                CustomLabelDetail labelDetail = ParentKeycard.Base.Details.OfType<CustomLabelDetail>().FirstOrDefault();
                if (labelDetail != null)
                {
                    try
                    {
                        LabelColorsDic.TryAdd(ParentKeycard.Serial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomLabelDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the text printed on the label of the <see cref="Keycard"/>.
        /// </summary>
        public string LabelText
        {
            get
            {
                LabelTextDic.TryGetValue(ParentKeycard.Serial, out string value);
                return value;
            }
            set
            {
                CustomLabelDetail labelDetail = ParentKeycard.Base.Details.OfType<CustomLabelDetail>().FirstOrDefault();
                if (labelDetail != null)
                {
                    try
                    {
                        object[] args = { value, LabelColor };
                        ArraySegment<object> arguments = new(args);
                        labelDetail.SetArguments(arguments);
                        LabelTextDic.TryAdd(ParentKeycard.Serial, value);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"{nameof(CustomKeycard)}: Error processing CustomLabelDetail: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the color used when rendering permissions on the <see cref="Keycard"/>.
        /// Set this first before setting <see cref="Permissions"/>, or the color will not render.
        /// </summary>
        public Color32 PermissionsColor
        {
            get
            {
                PermissionColorsDic.TryGetValue(ParentKeycard.Serial, out Color32 value);
                return value;
            }
            set
            {
                PermissionColorsDic.TryAdd(ParentKeycard.Serial, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="KeycardLevels"/> granted by this <see cref="Keycard"/>.
        /// </summary>
        public KeycardLevels Permissions
        {
            get
            {
                PermissionsDic.TryGetValue(ParentKeycard.Serial, out KeycardLevels value);
                return value;
            }
            set
            {
                CustomPermsDetail permsDetail = ParentKeycard.Base.Details.OfType<CustomPermsDetail>().FirstOrDefault();
                if (permsDetail != null)
                {
                    try
                    {
                        object[] args = { value, PermissionsColor };
                        ArraySegment<object> arguments = new(args);
                        permsDetail.SetArguments(arguments);
                        PermissionsDic.TryAdd(ParentKeycard.Serial, value);
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
                SerialNumberDic.TryGetValue(ParentKeycard.Serial, out string value);
                return value;
            }
            set
            {
                CustomSerialNumberDetail serialNumberDetail = ParentKeycard.Base.Details.OfType<CustomSerialNumberDetail>().FirstOrDefault();
                if (serialNumberDetail != null && (ParentKeycard.Type == ItemType.KeycardCustomTaskForce || ParentKeycard.Type == ItemType.KeycardCustomMetalCase))
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        serialNumberDetail.SetArguments(arguments);
                        SerialNumberDic.TryAdd(ParentKeycard.Serial, value);
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
                WearIndexDic.TryGetValue(ParentKeycard.Serial, out byte value);
                return value;
            }
            set
            {
                CustomWearDetail wearDetail = ParentKeycard.Base.Details.OfType<CustomWearDetail>().FirstOrDefault();
                if (wearDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        wearDetail.SetArguments(arguments);
                        WearIndexDic.TryAdd(ParentKeycard.Serial, value);
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
                RankIndexDic.TryGetValue(ParentKeycard.Serial, out int value);
                return value;
            }
            set
            {
                CustomRankDetail rankDetail = ParentKeycard.Base.Details.OfType<CustomRankDetail>().FirstOrDefault();
                if (rankDetail != null)
                {
                    try
                    {
                        object[] args = { value };
                        ArraySegment<object> arguments = new(args);
                        rankDetail.SetArguments(arguments);
                        RankIndexDic.TryAdd(ParentKeycard.Serial, value);
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
