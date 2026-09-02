using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class DynamicSpawn
    {
        public virtual string Room { get; set; } = "Lcz330";
        public virtual Vector3 Coords { get; set; } = new Vector3(0, 0, 0);
    }
}