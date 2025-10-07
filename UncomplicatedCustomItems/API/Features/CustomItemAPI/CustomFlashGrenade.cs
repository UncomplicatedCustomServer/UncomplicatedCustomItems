using MapGeneration;
using CustomPlayerEffects;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;
using UncomplicatedCustomItems.API.Components;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomFlashGrenade : APICustomItem
    {
        /// <summary>
        /// Gets or sets the minimum duration of player can take the effect.
        /// </summary>
        public abstract float MinimalDurationEffect { get; set; }

        /// <summary>
        /// Gets or sets the additional duration of the <see cref="Blindness"/> effect.
        /// </summary>
        public abstract float AdditionalBlindedEffect { get; set; }

        /// <summary>
        /// Gets or sets the how mush the flash grenade going to be intensified when explode at <see cref="RoomName.Surface"/>.
        /// </summary>
        public abstract float SurfaceDistanceIntensifier { get; set; }

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
                if (ExplodeOnImpact)
                    ev.Projectile.GameObject.AddComponent<CollisionHandler>().Init((ev.Player ?? LabApi.Features.Wrappers.Player.Host).GameObject, ev.Projectile.Base);         
            }
        }


        protected virtual void OnDetonating(ProjectileExplodingEventArgs ev) { }
        protected virtual void OnDetonated(ProjectileExplodedEventArgs ev) { }
        protected virtual void OnThrowing(PlayerThrowingProjectileEventArgs ev) { }
        protected virtual void OnThrown(PlayerThrewProjectileEventArgs ev) { }
    }
}