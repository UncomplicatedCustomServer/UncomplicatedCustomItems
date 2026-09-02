using UncomplicatedCustomItems.API.Features;
using System;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class CheckedCustomFlagEventArgs : EventArgs
    {
        public CustomItem CustomItem { get; }
        public Type Flag { get; }
        public bool Passed { get; }

        public CheckedCustomFlagEventArgs(CustomItem customItem, Type flag, bool passed)
        {
            CustomItem = customItem;
            Flag = flag;
            Passed = passed;
        }
    }
}