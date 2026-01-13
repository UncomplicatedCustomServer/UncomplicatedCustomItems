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
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerChangingAttachmentsEventArgs ev)
            {
                if (Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out var item))
                {
                    if (item.TryGetModule<WorkstationBanHintOverride>(out var hintOverride))
                    {
                        ev.Player.SendHint(hintOverride.HintOverride.Replace("%name%", CustomItem.Name), hintOverride.DurationOverride);                        
                    }
                    else
                        ev.Player.SendHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);

                    ev.IsAllowed = false;
                }
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