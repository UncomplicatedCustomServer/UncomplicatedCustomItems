using Exiled.API.Enums;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface ISpawn
    {
        public abstract bool DoSpawn { get; set; }

        public abstract uint Count { get; set; }

        public abstract bool? PedestalSpawn { get; set; }
        
        public abstract List<Vector3> Coords { get; set; }

        public abstract List<DynamicSpawn> DynamicSpawn { get; set; }

        public abstract List<ZoneType> Zones { get; set; }

        public abstract bool ReplaceExistingPickup { get; set; }

        public abstract bool ForceItem { get; set; }
    }
}