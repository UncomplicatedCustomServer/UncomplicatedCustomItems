using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Elements.SpecificData;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;

#nullable enable
namespace UncomplicatedCustomItems.Elements
{
    public class CustomItem : ICustomItem
    {
        /// <summary>
        /// The unique Id of the Custom Item. Can't be <= 0
        /// </summary>
        public uint Id { get; set; } = 1;

        /// <summary>
        /// The Name of the object. Can appears when for example you pick it up
        /// </summary>
        [Description("The name of the custom item")]
        public string Name { get; set; } = "Detonator";

        /// <summary>
        /// The description. Useful at the moment ig
        /// </summary>
        [Description("The description of the custom item")]
        public string Description { get; set; } = "11/09/20 COFF COFF uwu uwu uwu";

        /// <summary>
        /// The weight of the item
        /// </summary>
        [Description("The weight of the custom item")]
        public float Weight { get; set; } = 2f;

        /// <summary>
        /// The <see cref="ItemType"/> (Base) of the Custom Item
        /// </summary>
        [Description("The Item base for the custom item")]
        public ItemType Item { get; set; } = ItemType.Coin;

        /// <summary>
        /// The Scale of the Custom Item. If 0, 0, 0 then it's disabled
        /// </summary>
        [Description("The scale of the custom item, 0 0 0 means disabled")]
        public Vector3 Scale { get; set; } = Vector3.one;

        /// <summary>
        /// The <see cref="CustomItemType"/> of the Custom Item
        /// </summary>
        public CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        /// <summary>
        /// The <see cref="IData">Custom Data</see>, based on the CustomItemType
        /// </summary>
        public IData CustomData { get; set; } = new ItemData();
    }
}