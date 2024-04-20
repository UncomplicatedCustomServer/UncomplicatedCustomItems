using UncomplicatedCustomItems.Elements;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
#nullable enable
    public interface IItemData : IData
    {
        public abstract ItemEvents Event { get; set; }

        public abstract string? Command { get; set; }

        public abstract Response? Response { get; set; }

        public abstract bool EventsEnabled { get; set; }
    }
}
