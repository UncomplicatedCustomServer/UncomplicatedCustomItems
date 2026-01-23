using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class LifeSteal : CustomModuleBase
    {
        public override string Name => "LifeSteal";
        public override List<string> RequiredArguments =>
        [
            "PercentageBased",
            "LifeStealAmount",
            "LifeStealPercentage",
        ];

        public bool PercentageBased { get; set; }
        public float LifeStealAmount { get; set; }
        public float LifeStealPercentage { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<bool>("PercentageBased", out var percentageBased))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} PercentageBased is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<float>("LifeStealAmount", out var lifeStealAmount))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} LifeStealAmount is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("LifeStealPercentage", out var lifeStealPercentage))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} LifeStealPercentage is not a valid float!");
                    return;
                }

                PercentageBased = percentageBased;
                LifeStealAmount = lifeStealAmount;
                LifeStealPercentage = lifeStealPercentage;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerHurtEventArgs ev)
            {
                foreach (Dictionary<object, object> args in Arguments)
                {
                    if (PercentageBased && ev.DamageHandler is AttackerDamageHandler handler)
                    {
                        float healAmount = handler.Damage * (LifeStealPercentage / 100f);
                        ev.Attacker.Heal(healAmount);
                        LogManager.Debug($"Healed {ev.Attacker.Nickname} for {healAmount} health based on {LifeStealPercentage}% of damage dealt.");
                    }
                    else
                    {
                        ev.Attacker.Heal(LifeStealAmount);
                        LogManager.Debug($"Healed {ev.Attacker.Nickname} for {LifeStealAmount} health.");
                    }
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
