using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface ISpawn
    {
        public abstract bool DoSpawn { get; set; }

        public abstract uint Count { get; set; }

        public List<SpawnData> SpawnSettings { get; set; }
    }
}