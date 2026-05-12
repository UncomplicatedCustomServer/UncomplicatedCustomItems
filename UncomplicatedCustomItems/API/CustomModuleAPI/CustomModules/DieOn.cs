using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DieOn : CustomModuleBase
    {
        public override string Name => "DieOn";
        public override List<string> RequiredArguments =>
        [
            "Vaporize",
            "DeathMessage",
        ];

        public string DeathMessage { get; set; } = string.Empty;
        public bool Vaporize { get; set; }
        public TriggerOn Trigger { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("DeathMessage", out var deathMessage))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} DeathMessage is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<bool>("Vaporize", out var vaporize))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Vaporize is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                DeathMessage = deathMessage;
                Vaporize = vaporize;
                Trigger = trigger;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (CustomItem == null)
                return;

            if (eventArgs is PlayerUsedItemEventArgs playerUsedItem && HasFlagFast(Trigger, TriggerOn.OnUse))
            {
                if (Vaporize)
                    playerUsedItem.Player.Vaporize();
                else
                    playerUsedItem.Player.Kill($"{DeathMessage.Replace("%name%", CustomItem.Name)}");
                    
            }

            if (eventArgs is PlayerDroppedItemEventArgs playerDroppedItem && HasFlagFast(Trigger, TriggerOn.OnDropped))
            {
                if (Vaporize)
                    playerDroppedItem.Player.Vaporize();
                else
                    playerDroppedItem.Player.Kill($"{DeathMessage.Replace("%name%", CustomItem.Name)}");
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