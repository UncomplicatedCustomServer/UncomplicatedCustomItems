using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
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
            PlayerEvents.ReloadingWeapon += instance.OnOwnerReloading;
            PlayerEvents.ReloadedWeapon += instance.OnOwnerReloaded;
            PlayerEvents.Dying += instance.OnOwnerDying;
            PlayerEvents.Death += instance.OnOwnerDied;
            PlayerEvents.Hurting += instance.OnOwnerHurting;
            PlayerEvents.Hurt += instance.OnOwnerHurt;
            PlayerEvents.FlippingCoin += instance.OnOwnerFlippingCoin;
            PlayerEvents.FlippedCoin += instance.OnOwnerFlippedCoin;
            PlayerEvents.Escaping += instance.OnOwnerEscaping;
            PlayerEvents.Escaped += instance.OnOwnerEscaped;
            PlayerEvents.DryFiringWeapon += instance.OnOwnerDryFiring;
            PlayerEvents.DryFiredWeapon += instance.OnOwnerDryFired;
            PlayerEvents.DroppingItem += instance.OnOwnerDroppingItem;
            PlayerEvents.DroppedItem += instance.OnOwnerDroppedItem;
            PlayerEvents.ChangingItem += instance.OnOwnerChangingItem;
            PlayerEvents.ChangedItem += instance.OnOwnerChangedItem;
            ServerEvents.ProjectileExploding += instance.OnProjectileExploding;
            ServerEvents.ProjectileExploded += instance.OnProjectileExploded;
        }

        public static void Dispose()
        {
            PlayerEvents.ShotWeapon -= instance.OnShot;
            PlayerEvents.ShootingWeapon -= instance.OnShooting;
            PlayerEvents.UsedItem -= instance.OnItemUsed;
            PlayerEvents.UsingItem -= instance.OnItemUsing;
            PlayerEvents.ReloadingWeapon -= instance.OnOwnerReloading;
            PlayerEvents.ReloadedWeapon -= instance.OnOwnerReloaded;
            PlayerEvents.Dying -= instance.OnOwnerDying;
            PlayerEvents.Death -= instance.OnOwnerDied;
            PlayerEvents.Hurting -= instance.OnOwnerHurting;
            PlayerEvents.Hurt -= instance.OnOwnerHurt;
            PlayerEvents.FlippingCoin -= instance.OnOwnerFlippingCoin;
            PlayerEvents.FlippedCoin -= instance.OnOwnerFlippedCoin;
            PlayerEvents.Escaping -= instance.OnOwnerEscaping;
            PlayerEvents.Escaped -= instance.OnOwnerEscaped;
            PlayerEvents.DryFiringWeapon -= instance.OnOwnerDryFiring;
            PlayerEvents.DryFiredWeapon -= instance.OnOwnerDryFired;
            PlayerEvents.DroppingItem -= instance.OnOwnerDroppingItem;
            PlayerEvents.DroppedItem -= instance.OnOwnerDroppedItem;
            PlayerEvents.ChangingItem -= instance.OnOwnerChangingItem;
            PlayerEvents.ChangedItem -= instance.OnOwnerChangedItem;
            ServerEvents.ProjectileExploding -= instance.OnProjectileExploding;
            ServerEvents.ProjectileExploded -= instance.OnProjectileExploded;
            instance = null;
        }

        public virtual void OnShot(PlayerShotWeaponEventArgs ev) { }
        public virtual void OnShooting(PlayerShootingWeaponEventArgs ev) { }
        public virtual void OnItemUsed(PlayerUsedItemEventArgs ev) { }
        public virtual void OnItemUsing(PlayerUsingItemEventArgs ev) { }
        public virtual void OnOwnerReloading(PlayerReloadingWeaponEventArgs ev) { }
        public virtual void OnOwnerReloaded(PlayerReloadedWeaponEventArgs ev) { }
        public virtual void OnOwnerDying(PlayerDyingEventArgs ev) { }
        public virtual void OnOwnerDied(PlayerDeathEventArgs ev) { }
        public virtual void OnOwnerHurting(PlayerHurtingEventArgs ev) { }
        public virtual void OnOwnerHurt(PlayerHurtEventArgs ev) { }
        public virtual void OnOwnerFlippingCoin(PlayerFlippingCoinEventArgs ev) { }
        public virtual void OnOwnerFlippedCoin(PlayerFlippedCoinEventArgs ev) { }
        public virtual void OnOwnerEscaping(PlayerEscapingEventArgs ev) { }
        public virtual void OnOwnerEscaped(PlayerEscapedEventArgs ev) { }
        public virtual void OnOwnerDryFiring(PlayerDryFiringWeaponEventArgs ev) { }
        public virtual void OnOwnerDryFired(PlayerDryFiredWeaponEventArgs ev) { }
        public virtual void OnOwnerDroppingItem(PlayerDroppingItemEventArgs ev) { }
        public virtual void OnOwnerDroppedItem(PlayerDroppedItemEventArgs ev) { }
        public virtual void OnOwnerChangingItem(PlayerChangingItemEventArgs ev) { }
        public virtual void OnOwnerChangedItem(PlayerChangedItemEventArgs ev) { }
        public virtual void OnProjectileExploding(ProjectileExplodingEventArgs ev) { }
        public virtual void OnProjectileExploded(ProjectileExplodedEventArgs ev) { }
    }
}
