using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class HealOnKill : CustomModuleBase
    {
        public override string Name => "HealOnKill";

        public float HealAmount { get; set; }
        public bool ConvertToAhpIfFull { get; set; }

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
                    ev.Attacker.ArtificialHealth += HealAmount;
                }
                else
                {
                    ev.Attacker?.Heal(HealAmount);
                }
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