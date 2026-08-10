using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DieOn : CustomModuleBase
    {
        public override string Name => "DieOn";

        public string DeathMessage { get; set; } = string.Empty;
        public bool Vaporize { get; set; }
        public TriggerOn Trigger { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || CustomItem == null)
                return;

            string formattedMsg = DeathMessage.Replace("%name%", CustomItem.Name);

            if (eventArgs is PlayerUsedItemEventArgs playerUsedItem && HasFlagFast(Trigger, TriggerOn.OnUse))
            {
                if (Vaporize)
                {
                    playerUsedItem.Player.Vaporize();                    
                }
                else
                    playerUsedItem.Player.Kill(formattedMsg);
            }

            if (eventArgs is PlayerDroppedItemEventArgs playerDroppedItem && HasFlagFast(Trigger, TriggerOn.OnDropped))
            {
                if (Vaporize)
                {
                    playerDroppedItem.Player.Vaporize();                    
                }
                else
                    playerDroppedItem.Player.Kill(formattedMsg);
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.UsedItem += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.UsedItem -= Run;
        }
    }
}