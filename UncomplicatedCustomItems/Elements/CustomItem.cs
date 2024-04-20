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
        [Description("The Id of the custom item")]
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
        public string Description { get; set; } = "11/09/2001 uwu uwu uwu";

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
        public Vector3 Scale { get; set; } = new();

        /// <summary>
        /// The <see cref="CustomItemType"/> of the Custom Item
        /// </summary>
        public CustomItemType CustomItemType { get; set; } = CustomItemType.Item;

        /// <summary>
        /// The <see cref="IData">Custom Data</see>, based on the CustomItemType
        /// </summary>
        public IData CustomData { get; set; } = new ItemData();

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> inside the inventory of a player
        /// </summary>
        /// <param name="Player"></param>
        /// <returns>The <see cref="SummonedCustomItem"/> class</returns>
        public SummonedCustomItem Summon(Player Player)
        {
            return new(this, Player);
        }

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> as an existing pickup
        /// </summary>
        /// <param name="Pickup"></param>
        /// <returns>The <see cref="SummonedCustomItem"/> class</returns>
        public SummonedCustomItem Summon(Pickup Pickup)
        {
            return new(this, Pickup);
        }

        /// <summary>
        /// Summon the <see cref="ICustomItem"/> as a new pickup
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Rotation"></param>
        /// <returns>The <see cref="SummonedCustomItem"/> class</returns>
        public SummonedCustomItem Summon(Vector3 Position, Quaternion Rotation)
        {
            return new(this, Position, Rotation);
        }
    }
}