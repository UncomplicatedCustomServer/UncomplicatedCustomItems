using LabApi.Events;
using UncomplicatedCustomItems.Events.Arguments.JailbirdEvents;

namespace UncomplicatedCustomItems.Events.Handlers
{
    public class JailbirdEvents
    {
        /// <summary>
        /// Event for Inpecting Items
        /// </summary>
        public static event LabEventHandler<ChangedWearStateEventArgs>? ChangedWearState;

        /// <summary>
        /// Event for Inspected Items
        /// </summary>
        public static event LabEventHandler<ChangingWearStateEventArgs>? ChangingWearState;

        internal static void OnWearStateChanged(ChangedWearStateEventArgs ev) => ChangedWearState?.Invoke(ev);

        internal static void OnWearStateChanging(ChangingWearStateEventArgs ev) => ChangingWearState?.Invoke(ev);
    }
}