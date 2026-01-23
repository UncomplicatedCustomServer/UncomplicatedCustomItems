using System;
using InventorySystem.Items.Jailbird;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class NoCharge : CustomModuleBase
    {
        public override string Name => "NoCharge";
        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerProcessingJailbirdMessageEventArgs ev)
            {
                if (ev.Message is JailbirdMessageType.ChargeStarted or JailbirdMessageType.ChargeLoadTriggered)
                    ev.JailbirdItem.Base.SendRpc(JailbirdMessageType.ChargeFailed);
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ProcessingJailbirdMessage += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ProcessingJailbirdMessage -= Run;
        }
    }
}