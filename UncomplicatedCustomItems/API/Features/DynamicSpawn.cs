using MapGeneration;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class DynamicSpawn : IDynamicSpawn
    {
        public virtual string Room { get; set; } = "Lcz330";
        public virtual int Chance { get; set; } = 30;
        public virtual Vector3 Coords { get; set; } = new Vector3(0, 0, 0);
    }
}
