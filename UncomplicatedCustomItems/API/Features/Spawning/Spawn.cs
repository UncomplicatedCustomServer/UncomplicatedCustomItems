using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    /// <summary>
    /// Spawn settings for <see cref="ICustomItem"/>.
    /// </summary>
    public class Spawn : ISpawn
    {
        /// <summary>
        /// Determines whether the item can naturally spawn.
        /// </summary>
        [Description("If true, the custom item can spawn. If false, it will not.")]
        public virtual bool DoSpawn { get; set; } = false;

        /// <summary>
        /// Specifies how many instances of this custom item should be spawned.
        /// </summary>
        [Description("The number of custom items to spawn.")]
        public virtual uint Count { get; set; } = 1;

        public virtual List<SpawnData> SpawnSettings { get; set; } =
        [
            new()
            {
                Chance = 30,
            }
        ];
    }
}
