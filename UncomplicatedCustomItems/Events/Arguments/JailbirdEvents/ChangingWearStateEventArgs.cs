using System;
using InventorySystem.Items.Jailbird;

namespace UncomplicatedCustomItems.Events.Arguments.JailbirdEvents
{
    public class ChangingWearStateEventArgs : EventArgs
    {
        public LabApi.Features.Wrappers.JailbirdItem JailbirdItem { get; }

        public JailbirdWearState NewWearState { get; set; }

        public JailbirdWearState OldWearState { get; }

        public LabApi.Features.Wrappers.Player Player { get; }

        public bool IsAllowed { get; set; }

        public ChangingWearStateEventArgs(LabApi.Features.Wrappers.JailbirdItem jailbird, JailbirdWearState newWearState, JailbirdWearState oldWearState, LabApi.Features.Wrappers.Player player, bool isAllowed = true)
        {
            JailbirdItem = jailbird;
            NewWearState = newWearState;
            OldWearState = oldWearState;
            Player = player;
            IsAllowed = isAllowed;
        }
    }
}