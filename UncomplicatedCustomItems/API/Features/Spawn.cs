using Exiled.API.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class Spawn : ISpawn
    {
        /// <summary>
        /// Determines whether the item can naturally spawn.
        /// </summary>
        [Description("If true, the custom item can spawn. If false, it will not.")]
        public bool DoSpawn { get; set; } = false;

        /// <summary>
        /// Specifies how many instances of this custom item should be spawned.
        /// </summary>
        [Description("The number of custom items to spawn.")]
        public uint Count { get; set; } = 1;

        /// <summary>
        /// The <see cref="Vector3"/> positions where the item is allowed to spawn.
        /// This is an array, allowing multiple values, with one being randomly chosen.
        /// If this is empty, the <see cref="Rooms"/> parameter will be used instead.
        /// </summary>
        [Description("Custom coordinates where the custom item will spawn.")]
        public List<Vector3> Coords { get; set; } = new();

        /// <summary>
        /// The <see cref="RoomType"/> locations where the item is allowed to spawn.
        /// If this is empty, the <see cref="Zones"/> parameter will be used instead.
        /// </summary>
        [Description("The room(s) where the custom item can spawn.")]
        public List<RoomType> Rooms { get; set; } = new();

        /// <summary>
        /// The <see cref="ZoneType"/> locations where the item is allowed to spawn.
        /// If <see cref="Rooms"/> is empty, this parameter will be used.
        /// </summary>
        [Description("The zone(s) where the custom item can spawn.")]
        public List<ZoneType> Zones { get; set; } = new()
        {
            ZoneType.HeavyContainment,
            ZoneType.Entrance
        };

        /// <summary>
        /// If true, this item will replace an existing pickup.
        /// If <see cref="ForceItem"/> is false, a random pickup will be replaced with this item.
        /// If false, the item will spawn on the floor of the room.
        /// </summary>
        [Description("If true, the custom item will replace an existing in-game item.")]
        public bool ReplaceExistingPickup { get; set; } = false;

        /// <summary>
        /// If true, this item will only replace another pickup of the same <see cref="ItemType"/> as the custom item.
        /// </summary>
        [Description("If true, this item will only replace another pickup of the same item type as the custom item.")]
        public bool ForceItem { get; set; } = false;
    }
}
