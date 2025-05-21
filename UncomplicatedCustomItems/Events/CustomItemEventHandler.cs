using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

namespace UncomplicatedCustomItems.Events
{
    public class CustomItemEventHandler
    {
        private static CustomItemEventHandler instance;
        public static void Init<T>() where T : CustomItemEventHandler, new()
        {
            instance = new T();
            PlayerEvents.ShotWeapon += instance.OnShot;
            PlayerEvents.ShootingWeapon += instance.OnShooting;
            PlayerEvents.UsedItem += instance.OnItemUsed;
            PlayerEvents.UsingItem += instance.OnItemUsing;
            PlayerEvents.ReloadingWeapon += instance.OnPlayerReloading;
            PlayerEvents.ReloadedWeapon += instance.OnPlayerReloaded;
            PlayerEvents.Dying += instance.OnPlayerDying;
            PlayerEvents.Death += instance.OnPlayerDied;
            PlayerEvents.Hurting += instance.OnPlayerHurting;
            PlayerEvents.Hurt += instance.OnPlayerHurt;
            PlayerEvents.FlippingCoin += instance.OnPlayerFlippingCoin;
            PlayerEvents.FlippedCoin += instance.OnPlayerFlippedCoin;
            PlayerEvents.Escaping += instance.OnPlayerEscaping;
            PlayerEvents.Escaped += instance.OnPlayerEscaped;
            PlayerEvents.DryFiringWeapon += instance.OnPlayerDryFiring;
            PlayerEvents.DryFiredWeapon += instance.OnPlayerDryFired;
        }

        public static void Dispose()
        {
            PlayerEvents.ShotWeapon -= instance.OnShot;
            PlayerEvents.ShootingWeapon -= instance.OnShooting;
            PlayerEvents.UsedItem -= instance.OnItemUsed;
            PlayerEvents.UsingItem -= instance.OnItemUsing;
            PlayerEvents.ReloadingWeapon -= instance.OnPlayerReloading;
            PlayerEvents.ReloadedWeapon -= instance.OnPlayerReloaded;
            PlayerEvents.Dying -= instance.OnPlayerDying;
            PlayerEvents.Death -= instance.OnPlayerDied;
            PlayerEvents.Hurting -= instance.OnPlayerHurting;
            PlayerEvents.Hurt -= instance.OnPlayerHurt;
            PlayerEvents.FlippingCoin -= instance.OnPlayerFlippingCoin;
            PlayerEvents.FlippedCoin -= instance.OnPlayerFlippedCoin;
            PlayerEvents.Escaping -= instance.OnPlayerEscaping;
            PlayerEvents.Escaped -= instance.OnPlayerEscaped;
            PlayerEvents.DryFiringWeapon -= instance.OnPlayerDryFiring;
            PlayerEvents.DryFiredWeapon -= instance.OnPlayerDryFired;
            instance = null;
        }
        public virtual void OnShot(PlayerShotWeaponEventArgs ev)
        {
        }
        public virtual void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
        }
        public virtual void OnItemUsed(PlayerUsedItemEventArgs ev)
        {
        }
        public virtual void OnItemUsing(PlayerUsingItemEventArgs ev)
        {
        }
        public virtual void OnPlayerReloading(PlayerReloadingWeaponEventArgs ev)
        {
        }
        public virtual void OnPlayerReloaded(PlayerReloadedWeaponEventArgs ev)
        {
        }
        public virtual void OnPlayerDying(PlayerDyingEventArgs ev)
        {
        }
        public virtual void OnPlayerDied(PlayerDeathEventArgs ev)
        {
        }
        public virtual void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
        }
        public virtual void OnPlayerHurt(PlayerHurtEventArgs ev)
        {
        }
        public virtual void OnPlayerFlippingCoin(PlayerFlippingCoinEventArgs)
        {
        }
        public virtual void OnPlayerFlippedCoin(PlayerFlippedCoinEventArgs ev)
        {
        }
        public virtual void OnPlayerEscaping(PlayerEscapingEventArgs ev)
        {
        }
        public virtual void OnPlayerEscaped(PlayerEscapedEventArgs ev)
        {
        }
        public virtual void OnPlayerDryFiring(PlayerDryFiringWeaponEventArgs ev)
        {
        }
        public virtual void OnPlayerDryFired(PlayerDryFiredWeaponEventArgs ev)
        {
        }
}
}
