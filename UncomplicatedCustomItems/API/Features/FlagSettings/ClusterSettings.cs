using System.Collections.Generic;
using UncomplicatedCustomItems.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class ClusterSettings : IClusterSettings
    {
        public ItemType ItemToSpawn { get; set; } = ItemType.GrenadeHE;
        public int? AmountToSpawn { get; set; } = 5;
        public float? ScpDamageMultiplier { get; set; } = 5f;
        public float? FuseTime { get; set; } = 5f;

    }
}