using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Elements.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.Elements
{
    public class YAMLCustomItem
    {
        public uint Id { get; set; } = 1;

        public string Name { get; set; } = "Detonator";

        public string Description { get; set; } = "An item that reminds me of 9/11";

        public float Weight { get; set; } = 2f;

        public ItemType Item { get; set; } = ItemType.Coin;

        public Vector3 Scale { get; set; } = Vector3.one;

        public CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        public Dictionary<string, string> CustomData { get; set; } = YAMLCaster.Encode(new ItemData());
    }
}
