using System;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Events.Arguments.CustomItemEvents
{
    public class CheckedCustomFlagEventArgs : EventArgs
    {
        public ICustomItem CustomItem { get; }
        public Type Flag { get; }
        public bool Passed { get; }

        public CheckedCustomFlagEventArgs(ICustomItem customItem, Type flag, bool passed)
        {
            CustomItem = customItem;
            Flag = flag;
            Passed = passed;
        }
    }
}