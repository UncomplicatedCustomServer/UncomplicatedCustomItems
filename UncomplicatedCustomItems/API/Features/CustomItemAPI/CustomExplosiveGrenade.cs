using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedCustomItems.API.Components;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomExplosiveGrenade : APICustomItem
    {
        /// <summary>
        /// Gets or sets the maximum radius of the grenade.
        /// </summary>
        public abstract float MaxRadius { get; set; }

        /// <summary>
        /// Gets or sets the multiplier for damage against <see cref="Team.SCPs"/> players.
        /// </summary>
        public abstract float ScpDamageMultiplier { get; set; }

        /// <summary>
        /// Gets or sets how long the <see cref="Burned"/> effect will last.
        /// </summary>
        public abstract float BurnDuration { get; set; }

        /// <summary>
        /// Gets or sets how long the <see cref="Deafened"/> effect will last.
        /// </summary>
        public abstract float DeafenDuration { get; set; }

        /// <summary>
        /// Gets or sets how long the <see cref="Concussed"/> effect will last.
        /// </summary>
        public abstract float ConcussDuration { get; set; }

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        public abstract float FuseTime { get; set; }

        /// <summary>
        /// Gets or sets wether or not the grenade will explode on impact
        /// </summary>
        public abstract bool ExplodeOnImpact { get; set; }

        /// <summary>
        /// Gets or sets the time to pull out the pin
        /// </summary>
        public abstract float PinPullTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether players can pickup grenade after throw.
        /// </summary>
        public abstract bool Repickable { get; set; }

        /// <summary>
        /// Gets or sets the player damage multiplier applied.
        /// </summary>
        public abstract float PlayerDamageMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the door damage multiplier applied.
        /// </summary>
        public abstract float DoorDamageMultiplier { get; set; }

        public override void RegisterEvents()
        {
            ServerEvent.ProjectileExploding += new LabApi.Events.LabEventHandler<ProjectileExplodingEventArgs>(InternalOnDetonating);
            ServerEvent.ProjectileExploded += new LabApi.Events.LabEventHandler<ProjectileExplodedEventArgs>(InternalOnDetonated);
            PlayerEvent.ThrowingProjectile += new LabApi.Events.LabEventHandler<PlayerThrowingProjectileEventArgs>(InternalOnThrowing);
            PlayerEvent.ThrewProjectile += new LabApi.Events.LabEventHandler<PlayerThrewProjectileEventArgs>(InternalOnThrown);

            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            ServerEvent.ProjectileExploding -= new LabApi.Events.LabEventHandler<ProjectileExplodingEventArgs>(InternalOnDetonating);
            ServerEvent.ProjectileExploded -= new LabApi.Events.LabEventHandler<ProjectileExplodedEventArgs>(InternalOnDetonated);
            PlayerEvent.ThrowingProjectile -= new LabApi.Events.LabEventHandler<PlayerThrowingProjectileEventArgs>(InternalOnThrowing);
            PlayerEvent.ThrewProjectile -= new LabApi.Events.LabEventHandler<PlayerThrewProjectileEventArgs>(InternalOnThrown);

            base.UnregisterEvents();
        }

        private void InternalOnDetonating(ProjectileExplodingEventArgs ev)
        {
            if (Check(ev.TimedGrenade))
                OnDetonating(ev);
        }

        private void InternalOnDetonated(ProjectileExplodedEventArgs ev)
        {
            if (Check(ev.TimedGrenade))
                OnDetonated(ev);
        }

        private void InternalOnThrowing(PlayerThrowingProjectileEventArgs ev)
        {
            if (Check(ev.ThrowableItem))
                OnThrowing(ev);
        }

        private void InternalOnThrown(PlayerThrewProjectileEventArgs ev)
        {
            if (Check(ev.ThrowableItem))
            {
                OnThrown(ev);
                if (ev.Projectile is TimedGrenadeProjectile timedprojectile)
                    timedprojectile.RemainingTime = FuseTime;
                if (ExplodeOnImpact)
                    ev.Projectile.GameObject.AddComponent<CollisionHandler>().Init((ev.Player ?? Player.Host).GameObject, ev.Projectile.Base);         
            }

        }

        protected virtual void OnDetonating(ProjectileExplodingEventArgs ev) { }
        protected virtual void OnDetonated(ProjectileExplodedEventArgs ev) { }
        protected virtual void OnThrowing(PlayerThrowingProjectileEventArgs ev) { }
        protected virtual void OnThrown(PlayerThrewProjectileEventArgs ev) { }
    }
}