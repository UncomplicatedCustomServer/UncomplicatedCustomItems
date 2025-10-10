using System;
using InventorySystem.Items.Jailbird;

namespace UncomplicatedCustomItems.Events.Arguments.JailbirdEvents
{
    public class ChangedWearStateEventArgs : EventArgs
    {
        public LabApi.Features.Wrappers.JailbirdItem JailbirdItem { get; }

        public JailbirdWearState NewWearState { get; }

        public JailbirdWearState OldWearState { get; }

        public LabApi.Features.Wrappers.Player Player { get; }

        public ChangedWearStateEventArgs(LabApi.Features.Wrappers.JailbirdItem jailbird, JailbirdWearState newWearState, JailbirdWearState oldWearState, LabApi.Features.Wrappers.Player player)
        {
            JailbirdItem = jailbird;
            NewWearState = newWearState;
            OldWearState = oldWearState;
            Player = player;
        }
    }
}