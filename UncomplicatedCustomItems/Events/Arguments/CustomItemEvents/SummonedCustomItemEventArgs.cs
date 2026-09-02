using UncomplicatedCustomItems.API.Features;
using System;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class SummonedCustomItemEventArgs : EventArgs
    {
        public CustomItem CustomItem { get; }

        public SummonedCustomItemEventArgs(CustomItem customItem)
        {
            CustomItem = customItem;
        }
    }
}