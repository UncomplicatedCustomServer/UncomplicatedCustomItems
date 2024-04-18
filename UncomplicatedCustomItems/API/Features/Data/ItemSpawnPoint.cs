using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features.Data
{
    public class ItemSpawnPoint : SpawnPoint
    {
        public override string Name { get; set; }

        public override float Chance { get; set; }

        public override Vector3 Position { get; set; }

        public SpawnLocationType Location { get; set; }
    }
}
