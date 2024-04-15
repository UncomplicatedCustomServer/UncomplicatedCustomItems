#nullable enable
using UncomplicatedCustomItems.Elements;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface ICustomItem
    {
        public abstract uint Id { get; set; }

        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        public abstract ItemType Item { get; set; }

        public abstract Vector3 Scale { get; set; }

        public abstract ItemEvents Event { get; set; }
        
        public abstract string? Command { get; set; }

        public abstract Response? Response { get; set; }

        public abstract bool EventsEnabled { get; set; }
    }
}
