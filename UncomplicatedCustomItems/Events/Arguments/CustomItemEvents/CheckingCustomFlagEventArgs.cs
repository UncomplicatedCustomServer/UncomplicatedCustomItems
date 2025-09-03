using System;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class CheckingCustomFlagEventArgs : EventArgs
    {
        public ICustomItem CustomItem { get; }

        public CustomFlags Flag { get; }

        public bool IsAllowed { get; }

        public CheckingCustomFlagEventArgs(ICustomItem customItem, CustomFlags customflag, bool isAllowed = true)
        {
            CustomItem = customItem;
            Flag = customflag;
            IsAllowed = isAllowed;
        }
    }
}