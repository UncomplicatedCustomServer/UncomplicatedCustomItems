using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class SpawnItemWhenDetonatedSettings : ISpawnItemWhenDetonatedSettings
    {
        [Description("Set this to UCI, ECI, or Normal.")]
        public string? ItemType { get; set; } = "";
        public uint? ItemId { get; set; } = 1;
        public float? TimeTillDespawn { get; set; } = 6f;
        public uint? Chance { get; set; } = 100;
        public bool? Pickupable { get; set; } = false;
    }
}