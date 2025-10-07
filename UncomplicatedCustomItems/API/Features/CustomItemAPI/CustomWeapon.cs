using System.Collections.Generic;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomWeapon : APICustomItem
    {
        /// <summary>
        /// Gets or sets the damage of the firearm. Negative to heal
        /// </summary>
        public abstract float Damage { get; set; }

        /// <summary>
        /// Gets or sets the max number of ammunitions
        /// </summary>
        public abstract int MaxAmmo { get; set; }

        /// <summary>
        /// Gets or sets the max number of ammunitions in the magazine
        /// </summary>
        public abstract int MaxMagazineAmmo { get; set; }

        /// <summary>
        /// Gets or sets the amount of ammo able to be chambered in the barrel.
        /// </summary>
        public abstract int MaxBarrelAmmo { get; set; }

        /// <summary>
        /// Gets or sets the penetration of the firearm
        /// </summary>
        public abstract float Penetration { get; set; }

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm
        /// </summary>
        public abstract float Inaccuracy { get; set; }

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm while the player is ADS
        /// </summary>
        public abstract float AimingInaccuracy { get; set; }

        /// <summary>
        /// Gets or sets the how much fast the value drop over the distance.
        /// </summary>
        public abstract float DamageFalloffDistance { get; set; }

        /// <summary>
        /// Gets or sets the weapon attachments.
        /// </summary>
        public abstract List<AttachmentName> Attachments { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="CustomItem"/> can damage the friendly team.
        /// </summary>
        public abstract bool EnableFriendlyFire { get; set; }

        public override void RegisterEvents()
        {
            PlayerEvent.ShotWeapon += new LabApi.Events.LabEventHandler<PlayerShotWeaponEventArgs>(InternalOnShot);
            PlayerEvent.ShootingWeapon += new LabApi.Events.LabEventHandler<PlayerShootingWeaponEventArgs>(InternalOnShooting);
            PlayerEvent.Hurt += new LabApi.Events.LabEventHandler<PlayerHurtEventArgs>(InternalOnHurt);
            PlayerEvent.Hurting += new LabApi.Events.LabEventHandler<PlayerHurtingEventArgs>(InternalOnHurting);
            PlayerEvent.ReloadedWeapon += new LabApi.Events.LabEventHandler<PlayerReloadedWeaponEventArgs>(InternalOnReloaded);
            PlayerEvent.ReloadingWeapon += new LabApi.Events.LabEventHandler<PlayerReloadingWeaponEventArgs>(InternalOnReloading);
            PlayerEvent.ChangingAttachments += new LabApi.Events.LabEventHandler<PlayerChangingAttachmentsEventArgs>(InternalOnChangingAttachments);
            PlayerEvent.ChangedAttachments += new LabApi.Events.LabEventHandler<PlayerChangedAttachmentsEventArgs>(InternalOnChangedAttachments);

            base.RegisterEvents();
        }

        public override void UnregisterEvents()
        {
            PlayerEvent.ShotWeapon -= new LabApi.Events.LabEventHandler<PlayerShotWeaponEventArgs>(InternalOnShot);
            PlayerEvent.ShootingWeapon -= new LabApi.Events.LabEventHandler<PlayerShootingWeaponEventArgs>(InternalOnShooting);
            PlayerEvent.Hurt -= new LabApi.Events.LabEventHandler<PlayerHurtEventArgs>(InternalOnHurt);
            PlayerEvent.Hurting -= new LabApi.Events.LabEventHandler<PlayerHurtingEventArgs>(InternalOnHurting);
            PlayerEvent.ReloadedWeapon -= new LabApi.Events.LabEventHandler<PlayerReloadedWeaponEventArgs>(InternalOnReloaded);
            PlayerEvent.ReloadingWeapon -= new LabApi.Events.LabEventHandler<PlayerReloadingWeaponEventArgs>(InternalOnReloading);
            PlayerEvent.ChangingAttachments -= new LabApi.Events.LabEventHandler<PlayerChangingAttachmentsEventArgs>(InternalOnChangingAttachments);
            PlayerEvent.ChangedAttachments -= new LabApi.Events.LabEventHandler<PlayerChangedAttachmentsEventArgs>(InternalOnChangedAttachments);

            base.UnregisterEvents();
        }

        private void InternalOnShooting(PlayerShootingWeaponEventArgs ev)
        {
            if (Check(ev.FirearmItem))
                OnShooting(ev);
        }

        private void InternalOnShot(PlayerShotWeaponEventArgs ev)
        {
            if (Check(ev.FirearmItem))
                OnShot(ev);
        }

        private void InternalOnHurt(PlayerHurtEventArgs ev)
        {
            if (Check(ev.Attacker))
                OnHurt(ev);

        }

        private void InternalOnHurting(PlayerHurtingEventArgs ev)
        {
            if (Check(ev.Attacker))
                OnHurting(ev);

        }

        private void InternalOnReloaded(PlayerReloadedWeaponEventArgs ev)
        {
            if (Check(ev.FirearmItem))
                OnReloaded(ev);

        }
        
        private void InternalOnReloading(PlayerReloadingWeaponEventArgs ev)
        {
            if (Check(ev.FirearmItem))
                OnReloading(ev);
        }
        
        private void InternalOnChangingAttachments(PlayerChangingAttachmentsEventArgs ev)
        {
            if (Check(ev.FirearmItem))
                OnChangingAttachments(ev);
        }
        
        private void InternalOnChangedAttachments(PlayerChangedAttachmentsEventArgs ev)
        {
            if (Check(ev.FirearmItem))
                OnChangedAttachments(ev);
        }
        

        protected virtual void OnShooting(PlayerShootingWeaponEventArgs ev) { }
        protected virtual void OnShot(PlayerShotWeaponEventArgs ev) { }
        protected virtual void OnHurt(PlayerHurtEventArgs ev) { }
        protected virtual void OnHurting(PlayerHurtingEventArgs ev) { }
        protected virtual void OnReloaded(PlayerReloadedWeaponEventArgs ev) { }
        protected virtual void OnReloading(PlayerReloadingWeaponEventArgs ev) { }
        protected virtual void OnChangingAttachments(PlayerChangingAttachmentsEventArgs ev) { }
        protected virtual void OnChangedAttachments(PlayerChangedAttachmentsEventArgs ev) { }
    }
}