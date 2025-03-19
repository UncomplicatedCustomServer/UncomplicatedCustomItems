using Exiled.API.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface ISpawn
    {
        public abstract bool DoSpawn { get; set; }

        public abstract uint Count { get; set; }
        
        public abstract List<Vector4> Coords { get; set; }

        public abstract List<RoomType> Rooms { get; set; }

        public abstract List<ZoneType> Zones { get; set; }

        public abstract bool ReplaceExistingPickup { get; set; }

        public abstract bool ForceItem { get; set; }
    }
}