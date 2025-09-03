using LabApi.Events;
using UncomplicatedCustomItems.Events.Arguments.CustomItemEvents;

namespace UncomplicatedCustomItems.Events.Handlers
{
#nullable enable
    public class CustomItemEvents
    {
        public static event LabEventHandler<SummonedCustomItemEventArgs>? SummonedCustomItem;

        public static event LabEventHandler<SummoningCustomItemEventArgs>? SummoningCustomItem;

        public static event LabEventHandler<CheckedCustomFlagEventArgs>? CheckedCustomFlag;

        public static event LabEventHandler<CheckingCustomFlagEventArgs>? CheckingCustomFlag;

        internal static void OnSummonedCustomItem(SummonedCustomItemEventArgs ev) => SummonedCustomItem?.Invoke(ev);

        internal static void OnSummoningCustomItem(SummoningCustomItemEventArgs ev) => SummoningCustomItem?.Invoke(ev);
        
        internal static void OnCheckedCustomFlag(CheckedCustomFlagEventArgs ev) => CheckedCustomFlag?.Invoke(ev);

        internal static void OnCheckingCustomFlag(CheckingCustomFlagEventArgs ev) => CheckingCustomFlag?.Invoke(ev);
    }
}