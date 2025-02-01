using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;

namespace UncomplicatedCustomItems.Elements
{
#nullable enable
    public class SpawnBehaviour
    {


        public bool do_spawn { get; set; } = false;

        public Vector3 coords { get; set; } = new Vector3(1, 1, 1);

        /// <summary>
        /// Gets or sets a <see cref="List{T}"/> of rooms that will be evaluated as spawnpoints
        /// </summary>
        public List<RoomType> rooms { get; set; } = new();

        /// <summary>
        /// Gets or sets a <see cref="List{T}"/> of zones that will be evaluated as spawnpoints
        /// </summary>
        public List<ZoneType> zones { get; set; } = new()
        {
            ZoneType.HeavyContainment,
            ZoneType.Entrance
        };
    }
}
