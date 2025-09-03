using System;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class SummonedCustomItemEventArgs : EventArgs
    {
        public ICustomItem CustomItem { get; }

        public SummonedCustomItemEventArgs(ICustomItem customItem)
        {
            CustomItem = customItem;
        }
    }
}