using System.Collections.Generic;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Enums;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class YAMLCustomItem
    {
        public uint Id { get; set; } = 1;

        public string Name { get; set; } = "Detonator";

        public string Description { get; set; } = "An item that reminds me of 9/11";

        public string BadgeName { get; set; } = "Janitor";

        public string BadgeColor { get; set; } = "pumpkin";

        public float Weight { get; set; } = 2f;

        public ItemType Item { get; set; } = ItemType.Coin;

        public Vector3 Scale { get; set; } = Vector3.one;

        public Spawn Spawn { get; set; } = new();

        public virtual CustomFlags? CustomFlags { get; set; } = null;

        public FlagSettings FlagSettings { get; set; } = new();

        public CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        public Dictionary<string, string> CustomData { get; set; } = YAMLCaster.Encode(new ItemData());
    }
}
