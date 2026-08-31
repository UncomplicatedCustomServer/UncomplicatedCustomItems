using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using PlayerStatsSystem;
using System;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class LifeSteal : CustomModuleBase
    {
        public override string Name => "LifeSteal";

        public bool PercentageBased { get; set; }
        public float LifeStealAmount { get; set; }
        public float LifeStealPercentage { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerHurtEventArgs ev)
            {
                if (PercentageBased && ev.DamageHandler is AttackerDamageHandler handler)
                {
                    float healAmount = handler.Damage * (LifeStealPercentage / 100f);
                    ev.Attacker?.Heal(healAmount);
                    LogManager.Debug($"Healed {ev.Attacker?.Nickname} for {healAmount} health based on {LifeStealPercentage}% of damage dealt.");
                }
                else
                {
                    ev.Attacker?.Heal(LifeStealAmount);
                    LogManager.Debug($"Healed {ev.Attacker?.Nickname} for {LifeStealAmount} health.");
                }
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.Hurt += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.Hurt -= Run;
        }
    }
}
