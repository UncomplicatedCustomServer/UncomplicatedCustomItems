using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DoNotTriggerTeslaGates : CustomModuleBase
    {
        public override string Name => "DoNotTriggerTeslaGates";

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerTriggeringTeslaEventArgs ev)
                ev.IsAllowed = false;
        }

        public override void RegisterEvents()
        {
            PlayerEvents.TriggeringTesla += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.TriggeringTesla -= Run;
        }
    }
}