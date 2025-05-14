using Exiled.API.Enums;
using MapGeneration;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    /// <summary>
    /// Spawn settings for <see cref="ICustomItem"/>.
    /// </summary>
    public class Spawn : ISpawn
    {
        /// <summary>
        /// Determines whether the item can naturally spawn.
        /// </summary>
        [Description("If true, the custom item can spawn. If false, it will not.")]
        public virtual bool DoSpawn { get; set; } = false;

        /// <summary>
        /// Specifies how many instances of this custom item should be spawned.
        /// </summary>
        [Description("The number of custom items to spawn.")]
        public virtual uint Count { get; set; } = 1;

        /// <summary>
        /// Specifies if the CustomItem will spawn in a SCP Pedestal.
        /// If this is false, the <see cref="Coords"/> parameter will be used instead.
        /// </summary>
        [Description("Replace a existing item in a SCP Pedestal with this CustomItem.")]
        public virtual bool? PedestalSpawn { get; set; } = false;

        /// <summary>
        /// The <see cref="Vector3"/> positions where the item is allowed to spawn.
        /// If this is empty, the <see cref="DynamicSpawn"/> parameter will be used instead.
        /// </summary>
        [Description("Custom coordinates where the custom item will spawn.")]
        public virtual List<Vector3> Coords { get; set; } = new();

        /// <summary>
        /// The <see cref="IDynamicSpawn"/> locations where the item is allowed to spawn.
        /// If this is empty, the <see cref="Zones"/> parameter will be used instead.
        /// </summary>
        [Description("The room(s) where the custom item can spawn.")]
        public virtual List<DynamicSpawn> DynamicSpawn { get; set; } =
        [
            new()
            {
                Room = RoomType.Lcz914,
                Chance = 30
            }
        ];

        /// <summary>
        /// The <see cref="ZoneType"/> locations where the item is allowed to spawn.
        /// If <see cref="DynamicSpawn"/> is empty, this parameter will be used.
        /// </summary>
        [Description("The zone(s) where the custom item can spawn.")]
        public virtual List<FacilityZone> Zones { get; set; } = new()
        {
            FacilityZone.HeavyContainment,
            FacilityZone.Entrance
        };

        /// <summary>
        /// If true, this item will replace an existing pickup.
        /// If <see cref="ForceItem"/> is false, a random pickup will be replaced with this item.
        /// If false, the item will spawn on the floor of the room.
        /// </summary>
        [Description("If true, the custom item will replace an existing in-game item.")]
        public virtual bool ReplaceExistingPickup { get; set; } = false;

        /// <summary>
        /// If true, this item will only replace another pickup of the same <see cref="ItemType"/> as the custom item.
        /// </summary>
        [Description("If true, this item will only replace another pickup of the same item type as the custom item.")]
        public virtual bool ForceItem { get; set; } = false;
    }
}
