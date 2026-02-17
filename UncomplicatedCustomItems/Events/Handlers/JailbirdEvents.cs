using LabApi.Events;
using UncomplicatedCustomItems.Events.Arguments.JailbirdEvents;

namespace UncomplicatedCustomItems.Events.Handlers
{
    public class JailbirdEvents
    {
#nullable enable
        /// <summary>
        /// Event for when a Jailbird's WearState has changed.
        /// </summary>
        public static event LabEventHandler<ChangedWearStateEventArgs>? ChangedWearState;

        /// <summary>
        /// Event for when a Jailbird's WearState is changing.
        /// </summary>
        public static event LabEventHandler<ChangingWearStateEventArgs>? ChangingWearState;

        internal static void OnWearStateChanged(ChangedWearStateEventArgs ev) => ChangedWearState?.Invoke(ev);

        internal static void OnWearStateChanging(ChangingWearStateEventArgs ev) => ChangingWearState?.Invoke(ev);
    }
}