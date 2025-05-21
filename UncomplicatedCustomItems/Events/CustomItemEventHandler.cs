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
            PlayerEvents.ReloadingWeapon += instance.OnPlayerReloading;
            PlayerEvents.ReloadedWeapon += instance.OnPlayerReloaded;
            PlayerEvents.Dying += instance.OnPlayerDying;
            PlayerEvents.Died += instance.OnPlayerDied;
            PlayerEvents.Hurting += instance.OnPlayerHurting;
            PlayerEvents.Hurt += instance.OnPlayerHurt;
            PlayerEvents.FlippingCoin += instance.OnPlayerFlippingCoin;
            LabAPIPlayerEvent.FlippedCoin += instance.OnPlayerFlippedCoin;
            PlayerEvents.Escaping += instance.OnPlayerEscaping;
            PlayerEvents.Escaped += instance.OnPlayerEscaped;
            PlayerEvents.DryfiringWeapon += instance.OnPlayerDryFiring;
            LabAPIPlayerEvent.DryFiredWeapon += instance.OnPlayerDryFired;
        }

        public static void Dispose()
        {
            PlayerEvents.Shot -= instance.OnShot;
            PlayerEvents.Shooting-= instance.OnShooting;
            PlayerEvents.UsedItem -= instance.OnItemUsed;
            PlayerEvents.UsingItem -= instance.OnItemUsing;
            PlayerEvents.ReloadingWeapon -= instance.OnPlayerReloading;
            PlayerEvents.ReloadedWeapon -= instance.OnPlayerReloaded;
            PlayerEvents.Dying -= instance.OnPlayerDying;
            PlayerEvents.Died -= instance.OnPlayerDied;
            PlayerEvents.Hurting -= instance.OnPlayerHurting;
            PlayerEvents.Hurt -= instance.OnPlayerHurt;
            PlayerEvents.FlippingCoin -= instance.OnPlayerFlippingCoin;
            LabAPIPlayerEvent.FlippedCoin -= instance.OnPlayerFlippedCoin;
            PlayerEvents.Escaping -= instance.OnPlayerEscaping;
            PlayerEvents.Escaped -= instance.OnPlayerEscaped;
            PlayerEvents.DryfiringWeapon -= instance.OnPlayerDryFiring;
            LabAPIPlayerEvent.DryFiredWeapon -= instance.OnPlayerDryFired;
            instance = null;
        }
        public virtual void OnShot(ShotEventArgs ev)
        {
        }
        public virtual void OnShooting(ShootingEventArgs ev)
        {
        }
        public virtual void OnItemUsed(UsedItemEventArgs ev)
        {
        }
        public virtual void OnItemUsing(UsingItemEventArgs ev)
        {
        }
        public virtual void OnPlayerReloading(ReloadingWeaponEventArgs ev)
        {
        }
        public virtual void OnPlayerReloaded(ReloadedWeaponEventArgs ev)
        {
        }
        public virtual void OnPlayerDying(DyingEventArgs ev)
        {
        }
        public virtual void OnPlayerDied(DiedEventArgs ev)
        {
        }
        public virtual void OnPlayerHurting(HurtingEventArgs ev)
        {
        }
        public virtual void OnPlayerHurt(HurtEventArgs ev)
        {
        }
        public virtual void OnPlayerFlippingCoin(FlippingCoinEventArgs ev)
        {
        }
        public virtual void OnPlayerFlippedCoin(PlayerFlippedCoinEventArgs ev)
        {
        }
        public virtual void OnPlayerEscaping(EscapingEventArgs ev)
        {
        }
        public virtual void OnPlayerEscaped(EscapedEventArgs ev)
        {
        }
        public virtual void OnPlayerDryFiring(DryfiringWeaponEventArgs ev)
        {
        }
        public virtual void OnPlayerDryFired(PlayerDryFiredWeaponEventArgs ev)
        {
        }
    }
}
