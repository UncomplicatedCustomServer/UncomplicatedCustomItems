using UnityEngine;

namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface IDynamicSpawn
    {
        public abstract string Room { get; set; }
        public abstract Vector3 Coords { get; set; }
    }
}
