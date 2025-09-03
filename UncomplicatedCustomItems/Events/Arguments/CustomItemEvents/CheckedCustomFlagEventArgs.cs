using System;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class CheckedCustomFlagEventArgs : EventArgs
    {
        public ICustomItem CustomItem { get; }
        public CustomFlags Flag { get; }
        public bool Passed { get; }

        public CheckedCustomFlagEventArgs(ICustomItem customItem, CustomFlags customflag)
        {
            CustomItem = customItem;
            Flag = customflag;

            API.Features.CustomItem item = customItem as API.Features.CustomItem;
            Passed = item.HasModule(customflag);
        }
    }
}