using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface IDynamicSpawn
    {
        public abstract RoomName Room { get; set; }
        public abstract int Chance { get; set; }
        public abstract Vector3 Coords { get; set; }
    }
}
