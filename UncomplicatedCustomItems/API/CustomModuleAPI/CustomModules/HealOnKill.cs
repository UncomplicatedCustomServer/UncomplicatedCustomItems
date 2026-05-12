using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class HealOnKill : CustomModuleBase
    {
        public override string Name => "HealOnKill";
        public override List<string> RequiredArguments => 
        [
            "HealAmount",
            "ConvertToAhpIfFull"
        ];

        public float HealAmount { get; set; }
        public bool ConvertToAhpIfFull { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<float>("HealAmount", out var healAmount))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} HealAmount is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<bool>("ConvertToAhpIfFull", out var convertToAhpIfFull))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ConvertToAhpIfFull is not a valid bool!");
                    return;
                }

                HealAmount = healAmount;
                ConvertToAhpIfFull = convertToAhpIfFull;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerDeathEventArgs ev)
            {
                if (!Utilities.TryGetSummonedCustomItem(ev.Attacker?.CurrentItem?.Serial ?? 0, out var item))
                    return;

                if (ev.Attacker?.Health >= ev.Attacker?.MaxHealth && ConvertToAhpIfFull)
                {
                    ev.Attacker?.ArtificialHealth += HealAmount;
                }
                else
                    ev.Attacker?.Heal(HealAmount);
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.Death += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.Death -= Run;
        }
    }
}