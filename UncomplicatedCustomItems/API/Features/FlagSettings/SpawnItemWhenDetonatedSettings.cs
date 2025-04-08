using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class SpawnItemWhenDetonatedSettings : ISpawnItemWhenDetonatedSettings
    {
        public ItemType ItemToSpawn { get; set; } = ItemType.SCP244a;
        public float? TimeTillDespawn { get; set; } = 6f;
    }
}