using PlayerEvents = Exiled.Events.Handlers.Player;
using LabAPIPlayerEvent = LabApi.Events.Handlers.PlayerEvents;
using Exiled.Events.EventArgs.Player;
using LabApi.Events.Arguments.PlayerEvents;

namespace UncomplicatedCustomItems.Events
{
    public class CustomItemEventHandler
    {
        private static CustomItemEventHandler instance;
        public static void Init<T>() where T : CustomItemEventHandler, new()
        {
            instance = new T();
            PlayerEvents.Shot += instance.OnShot;
            PlayerEvents.Shooting += instance.OnShooting;
            PlayerEvents.UsedItem += instance.OnItemUsed;
            PlayerEvents.UsingItem += instance.OnItemUsing;
            PlayerEvents.ReloadingWeapon += instance.OnOwnerReloading;
            PlayerEvents.ReloadedWeapon += instance.OnOwnerReloaded;
            PlayerEvents.Dying += instance.OnOwnerDying;
            PlayerEvents.Died += instance.OnOwnerDied;
            PlayerEvents.Hurting += instance.OnOwnerHurting;
            PlayerEvents.Hurt += instance.OnOwnerHurt;
            PlayerEvents.FlippingCoin += instance.OnOwnerFlippingCoin;
            LabAPIPlayerEvent.FlippedCoin += instance.OnOwnerFlippedCoin;
            PlayerEvents.Escaping += instance.OnOwnerEscaping;
            PlayerEvents.Escaped += instance.OnOwnerEscaped;
            PlayerEvents.DryfiringWeapon += instance.OnOwnerDryFiring;
            LabAPIPlayerEvent.DryFiredWeapon += instance.OnOwnerDryFired;
        }

        public static void Dispose()
        {
            PlayerEvents.Shot -= instance.OnShot;
            PlayerEvents.Shooting-= instance.OnShooting;
            PlayerEvents.UsedItem -= instance.OnItemUsed;
            PlayerEvents.UsingItem -= instance.OnItemUsing;
            PlayerEvents.ReloadingWeapon -= instance.OnOwnerReloading;
            PlayerEvents.ReloadedWeapon -= instance.OnOwnerReloaded;
            PlayerEvents.Dying -= instance.OnOwnerDying;
            PlayerEvents.Died -= instance.OnOwnerDied;
            PlayerEvents.Hurting -= instance.OnOwnerHurting;
            PlayerEvents.Hurt -= instance.OnOwnerHurt;
            PlayerEvents.FlippingCoin -= instance.OnOwnerFlippingCoin;
            LabAPIPlayerEvent.FlippedCoin -= instance.OnOwnerFlippedCoin;
            PlayerEvents.Escaping -= instance.OnOwnerEscaping;
            PlayerEvents.Escaped -= instance.OnOwnerEscaped;
            PlayerEvents.DryfiringWeapon -= instance.OnOwnerDryFiring;
            LabAPIPlayerEvent.DryFiredWeapon -= instance.OnOwnerDryFired;
            instance = null;
        }
        public virtual void OnShot(ShotEventArgs ev) { }
        public virtual void OnShooting(ShootingEventArgs ev) { }
        public virtual void OnItemUsed(UsedItemEventArgs ev) { }
        public virtual void OnItemUsing(UsingItemEventArgs ev) { }
        public virtual void OnOwnerReloading(ReloadingWeaponEventArgs ev) { }
        public virtual void OnOwnerReloaded(ReloadedWeaponEventArgs ev) { }
        public virtual void OnOwnerDying(DyingEventArgs ev) { }
        public virtual void OnOwnerDied(DiedEventArgs ev) { }
        public virtual void OnOwnerHurting(HurtingEventArgs ev) { }
        public virtual void OnOwnerHurt(HurtEventArgs ev) { }
        public virtual void OnOwnerFlippingCoin(FlippingCoinEventArgs ev) { }
        public virtual void OnOwnerFlippedCoin(PlayerFlippedCoinEventArgs ev) { }
        public virtual void OnOwnerEscaping(EscapingEventArgs ev) { }
        public virtual void OnOwnerEscaped(EscapedEventArgs ev) { }
        public virtual void OnOwnerDryFiring(DryfiringWeaponEventArgs ev) { }
        public virtual void OnOwnerDryFired(PlayerDryFiredWeaponEventArgs ev) { }
    }
}
