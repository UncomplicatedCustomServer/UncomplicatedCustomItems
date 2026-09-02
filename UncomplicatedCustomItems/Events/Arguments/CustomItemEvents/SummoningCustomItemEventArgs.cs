using UncomplicatedCustomItems.API.Features;
using System;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class SummoningCustomItemEventArgs : EventArgs
    {
        public CustomItem CustomItem { get; }
        public bool IsAllowed { get; }

        public SummoningCustomItemEventArgs(CustomItem customItem, bool isAllowed = true)
        {
            CustomItem = customItem;
            IsAllowed = isAllowed;
        }
    }
}