using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
#nullable enable

    internal class ItemData : IItemData
    {
        /// <summary>
        /// The <see cref="ItemEvents"/> of the object
        /// </summary>
        public ItemEvents Event { get; set; } = ItemEvents.Pickup;

        /// <summary>
        /// The command (<see cref="string>"/>) that will be executed when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public string? Command { get; set; } = null;

        /// <summary>
        /// The <see cref="Elements.Response"/> that will be executed when the <see cref="ItemEvents"/> will be fired
        /// </summary>
        public Response? Response { get; set; } = new();

        /// <summary>
        /// Can this item's events be used from APIs by third parties?
        /// </summary>
        public bool EventsEnabled { get; set; } = true;
    }
}
