using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ExplosiveBullets : CustomModuleBase
    {
        public override string Name => "ExplosiveBullets";
        public override List<string> RequiredArguments =>
        [
            "DamageRadius"
        ];

        public float DamageRadius { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<float>("DamageRadius", out var damageRadius))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} DamageRadius is not a valid float!");
                    return;
                }

                DamageRadius = damageRadius;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            base.Run(eventArgs);
            if (eventArgs is PlayerPlacedBulletHoleEventArgs playerPlacedBullet)
            {
                ExplosiveGrenadeProjectile grenade = (ExplosiveGrenadeProjectile)TimedGrenadeProjectile.SpawnActive(playerPlacedBullet.HitPosition, ItemType.GrenadeHE, playerPlacedBullet.Player, 0.2);
                grenade.MaxRadius = DamageRadius;
                grenade.FuseEnd();
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.PlacedBulletHole += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.PlacedBulletHole -= Run;
        }
    }
}