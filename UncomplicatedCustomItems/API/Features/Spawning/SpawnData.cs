using System.Collections.Generic;
using System.ComponentModel;
using MapGeneration;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class SpawnData
    {
        /// <summary>
        /// The chance the CustomItem will spawn max: 100 min: 0
        /// </summary>
        public virtual float Chance { get; set; } = 30;

        /// <summary>
        /// The <see cref="Vector3"/> positions where the item is allowed to spawn.
        /// If this is empty, the <see cref="DynamicSpawn"/> parameter will be used instead.
        /// </summary>
        [Description("Custom coordinates where the custom item will spawn.")]
        public virtual Vector3 Coords { get; set; } = Vector3.zero;

        /// <summary>
        /// The rotation of the CustomItem when spawned
        /// </summary>
        [Description("The rotation of the CustomItem when spawned")]
        public virtual Vector4 Rotation { get; set; } = Vector4.zero;

        public virtual LockerSpawn LockerSettings { get; set; } = new();

        /// <summary>
        /// The <see cref="IDynamicSpawn"/> locations where the item is allowed to spawn.
        /// If this is empty, the <see cref="Zones"/> parameter will be used instead.
        /// </summary>
        [Description("The room(s) where the custom item can spawn.")]
        public virtual List<DynamicSpawn> DynamicSpawn { get; set; } =
        [
            new()
            {
                Room = "Lcz914",
                Coords = new(1, 1, 1)
            }
        ];

        /// <summary>
        /// The <see cref="FacilityZone"/> locations where the item is allowed to spawn.
        /// If <see cref="DynamicSpawn"/> is empty, this parameter will be used.
        /// </summary>
        [Description("The zone(s) where the custom item can spawn.")]
        public virtual List<FacilityZone> Zones { get; set; } =
        [
            FacilityZone.HeavyContainment,
            FacilityZone.Entrance
        ];

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

        [Description("If true, this item can replace items in SCP Pedestals.")]
        public virtual bool? ReplaceItemsInPedestals { get; set; } = false;
    }
}