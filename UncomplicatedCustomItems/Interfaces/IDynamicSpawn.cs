using LabApi.Features.Wrappers;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface IDynamicSpawn
    {
        public abstract Room Room { get; set; }
        public abstract int Chance { get; set; }
        public abstract Vector3 Coords { get; set; }
    }
}
