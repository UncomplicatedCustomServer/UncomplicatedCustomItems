using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Enums;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class YAMLCustomItem
    {
        [Description("Sets the ID of the custom item. Custom items cannot share IDs.")]
        public uint Id { get; set; } = 1;

        [Description("Sets the name of the custom item.")]
        public string Name { get; set; } = "Detonator";

        [Description("Sets the description of the custom item. This is shown as part of a hint when the item is equipped or picked up.")]
        public string Description { get; set; } = "An item that reminds me of 9/11";

        [Description("Sets the badge name of the custom item. Remove the text in quotes to disable it.")]
        public string BadgeName { get; set; } = "Janitor";

        [Description("Sets the badge color of the custom item.")]
        public string BadgeColor { get; set; } = "pumpkin";

        [Description("Sets the weight of the custom item. This affects movement speed when equipped.")]
        public float Weight { get; set; } = 2f;

        [Description("Sets the item the custom item will use.")]
        public ItemType Item { get; set; } = ItemType.Coin;

        [Description("Sets the scale of the custom item when dropped.")]
        public Vector3 Scale { get; set; } = Vector3.one;

        [Description("Defines the spawn settings for the custom item. Information on rooms can be found in the UCI Information forum on Discord.")]
        public Spawn Spawn { get; set; } = new();

        [Description("Sets the custom flags of the custom item. Information about custom flags can be found in the UCI Information forum on Discord.")]
        public virtual CustomFlags? CustomFlags { get; set; } = null;

        [Description("Defines the flag settings for the custom item.")]
        public FlagSettings FlagSettings { get; set; } = new();

        [Description("Sets the custom data type the item will use.")]
        public CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        [Description("Specifies the modifications the custom item will have.")]
        public Dictionary<string, string> CustomData { get; set; } = YAMLCaster.Encode(new ItemData());
    }
}
