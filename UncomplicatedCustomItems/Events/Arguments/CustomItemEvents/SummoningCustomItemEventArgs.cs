using System;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class SummoningCustomItemEventArgs : EventArgs
    {
        public ICustomItem CustomItem { get; }
        public bool IsAllowed { get; }

        public SummoningCustomItemEventArgs(ICustomItem customItem, bool isAllowed = true)
        {
            CustomItem = customItem;
            IsAllowed = isAllowed;
        }
    }
}