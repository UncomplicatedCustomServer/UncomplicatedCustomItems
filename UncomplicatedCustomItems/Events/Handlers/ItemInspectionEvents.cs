using LabApi.Events;
using UncomplicatedCustomItems.Events.Arguments.ItemInspectionEvents;

namespace UncomplicatedCustomItems.Events.Handlers
{
    public class ItemInspectionEvents
    {
        /// <summary>
        /// Event for Inpecting Items
        /// </summary>
        public static event LabEventHandler<InspectingItemEventArgs>? InspectingItem;

        /// <summary>
        /// Event for Inspected Items
        /// </summary>
        public static event LabEventHandler<InspectedItemEventArgs>? InspectedItem;

        internal static void OnInspectingItem(InspectingItemEventArgs ev) => InspectingItem?.Invoke(ev);

        internal static void OnInspectedItem(InspectedItemEventArgs ev) => InspectedItem?.Invoke(ev);
    }
}