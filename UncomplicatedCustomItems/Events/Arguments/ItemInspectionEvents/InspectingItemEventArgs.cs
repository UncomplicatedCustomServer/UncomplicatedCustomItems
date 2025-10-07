using System;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents
{

    public class InspectingItemEventArgs(Item item, Player player, bool isAllowed = true) : EventArgs
    {
        /// <summary>
        /// The Inspected <see cref="LabApi.Features.Wrappers.Item"/>.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// The <see cref="LabApi.Features.Wrappers.Player"/> that inspected the <see cref="LabApi.Features.Wrappers.Item"/>.
        /// </summary>
        public Player Player { get; }
        
        /// <summary>
        /// Whether or not the Inspection is allowed to continue.
        /// </summary>
        public bool IsAllowed { get; }
    }
}