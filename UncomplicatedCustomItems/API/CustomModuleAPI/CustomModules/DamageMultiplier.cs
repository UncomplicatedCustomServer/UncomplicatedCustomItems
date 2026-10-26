using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using PlayerRoles;
using PlayerStatsSystem;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DamageMultiplier : CustomModuleBase
    {
        public override string Name => nameof(DamageMultiplier);

        public Dictionary<RoleTypeId, float> Multiplier { get; set; } = [];

        public override void Run(EventArgs ev)
        {
            if (!Check(ev))
                return;

            if (ev is PlayerHurtingEventArgs hurting)
            {
                if (hurting.DamageHandler is not StandardDamageHandler handler)
                    return;

                if (!Multiplier.TryGetValue(hurting.Player.Role, out var multi))
                    return;

                handler.Damage *= multi;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.Hurting += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.Hurting -= Run;
        }
    }
}