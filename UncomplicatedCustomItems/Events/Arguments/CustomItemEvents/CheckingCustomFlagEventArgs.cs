using UncomplicatedCustomItems.API.Features;
using System;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class CheckingCustomFlagEventArgs : EventArgs
    {
        public CustomItem CustomItem { get; }

        public Type Flag { get; }

        public bool IsAllowed { get; }

        public CheckingCustomFlagEventArgs(CustomItem customItem, Type customflag, bool isAllowed = true)
        {
            CustomItem = customItem;
            Flag = customflag;
            IsAllowed = isAllowed;
        }
    }
}