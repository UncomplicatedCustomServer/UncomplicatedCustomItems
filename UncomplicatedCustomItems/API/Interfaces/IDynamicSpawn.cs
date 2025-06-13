using Exiled.API.Enums;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface IDynamicSpawn
    {
        public abstract RoomType Room { get; set; }
        public abstract int Chance { get; set; }
        public abstract Vector3 Coords { get; set; }
    }
}
