using System;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class RunningCustomItemCommandEventArgs : EventArgs
    {
        public string ProcessedCommand { get; }
        public string RawCommand { get; }
        public SummonedCustomItem CustomItem { get; }
        public bool IsAllowed { get; }

        public RunningCustomItemCommandEventArgs(string processedCommand, string rawCommand, SummonedCustomItem customItem, bool isAllowed = true)
        {
            CustomItem = customItem;
            ProcessedCommand = processedCommand;
            RawCommand = rawCommand;
            IsAllowed = isAllowed;
        }
    }
}