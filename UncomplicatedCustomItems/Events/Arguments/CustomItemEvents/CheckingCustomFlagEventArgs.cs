using System;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class CheckingCustomFlagEventArgs : EventArgs
    {
        public ICustomItem CustomItem { get; }

        public Type Flag { get; }

        public bool IsAllowed { get; }

        public CheckingCustomFlagEventArgs(ICustomItem customItem, Type customflag, bool isAllowed = true)
        {
            CustomItem = customItem;
            Flag = customflag;
            IsAllowed = isAllowed;
        }
    }
}