using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ExplosiveBullets : CustomModuleBase
    {
        public override string Name => "ExplosiveBullets";

        public float DamageRadius { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerPlacedBulletHoleEventArgs playerPlacedBullet)
            {
                ExplosiveGrenadeProjectile? grenade = (ExplosiveGrenadeProjectile?)TimedGrenadeProjectile.SpawnActive(playerPlacedBullet.HitPosition, ItemType.GrenadeHE, playerPlacedBullet.Player, 0.2);
                if (grenade != null)
                {
                    grenade.MaxRadius = DamageRadius;
                    grenade.FuseEnd();
                }
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