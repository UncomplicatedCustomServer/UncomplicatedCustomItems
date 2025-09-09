using System;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class RanCustomItemCommandEventArgs : EventArgs
    {
        public string ProcessedCommand { get; }
        public string RawCommand { get; }
        public SummonedCustomItem CustomItem { get; }

        public RanCustomItemCommandEventArgs(string processedCommand, string rawCommand, SummonedCustomItem customItem)
        {
            CustomItem = customItem;
            ProcessedCommand = processedCommand;
            RawCommand = rawCommand;
        }
    }
}