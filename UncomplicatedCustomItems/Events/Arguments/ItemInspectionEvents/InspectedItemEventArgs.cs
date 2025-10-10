using System;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents
{
    public class InspectedItemEventArgs(Item item, Player player) : EventArgs
    {
        /// <summary>
        /// The Inspected <see cref="LabApi.Features.Wrappers.Item"/>.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// The <see cref="LabApi.Features.Wrappers.Player"/> that inspected the <see cref="LabApi.Features.Wrappers.Item"/>.
        /// </summary>
        public Player Player { get; }
    }
}