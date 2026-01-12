using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class WorkstationBan : CustomModuleBase
    {
        public override string Name => "WorkstationBan";

        public override void Run(EventArgs eventArgs)
        {
            base.Run(eventArgs);
            if (eventArgs is PlayerChangingAttachmentsEventArgs ev)
            {
                ev.Player.SendHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
                ev.IsAllowed = false;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ChangingAttachments += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ChangingAttachments -= Run;
        }
    }
}