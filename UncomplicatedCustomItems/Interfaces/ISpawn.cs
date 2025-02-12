using Exiled.API.Enums;
using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface ISpawn
    {
        public abstract bool DoSpawn { get; set; }

        public abstract uint Count { get; set; }
        
        public abstract List<Vector3> Coords { get; set; }

        public abstract List<RoomType> Rooms { get; set; }

        public abstract List<ZoneType> Zones { get; set; }

        public abstract bool ReplaceExistingPickup { get; set; }

        public abstract bool ForceItem { get; set; }
    }
}