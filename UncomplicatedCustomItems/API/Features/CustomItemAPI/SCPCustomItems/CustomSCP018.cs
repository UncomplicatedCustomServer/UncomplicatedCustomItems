using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Components;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP018 : APICustomItem
    {
        /// <summary>
        /// Gets or sets the time that the <see cref="CustomSCP018"/> instance will take to detonate.
        /// </summary>
        public float FuseTime { get; set; }

        /// <summary>
        /// Gets or sets the time that the <see cref="CustomSCP018"/> instance will take untill it enables friendly fire.
        /// </summary>
        public float FriendlyFireTime { get; set; }

        /// <summary>
        /// Gets or sets the time needed to throw the <see cref="CustomSCP018"/> instance.
        /// </summary>
        public float PinPullTime { get; set; }

        /// <summary>
        /// Gets or sets whether or not the <see cref="CustomSCP018"/> instance is pickupable after throwing.
        /// </summary>
        public bool Repickable { get; set; }

        /// <summary>
        /// Gets or sets whether or not the <see cref="CustomSCP018"/> instance will explode when impacting something.
        /// </summary>
        public bool ExplodeOnImpact { get; set; }

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