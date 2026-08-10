using Footprinting;
using InventorySystem.Items.Firearms.Extensions;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using System;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ItemShot : CustomModuleBase
    {
        public override string Name => "ItemShot";

        public bool IsGrenade { get; set; }
        public bool GrenadeExplodeOnImpact { get; set; }
        public bool IsCustomItem { get; set; }
        public float Velocity { get; set; }
        public float UpwardsFactor { get; set; }
        public Vector3 Torque { get; set; }
        public uint CustomItemId { get; set; }
        public ItemType ItemType { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || CustomItem == null)
                return;
                
            if (eventArgs is PlayerShootingWeaponEventArgs ev)
            {
                Vector3 position = ev.Player.Camera.position;
                if (BarrelTipExtension.TryFindWorldmodelBarrelTip(ev.FirearmItem.Serial, out var tip))
                    position = tip.WorldspacePosition;

                if (IsCustomItem)
                {
                    ICustomItem? customItem = Utilities.GetCustomItem(CustomItemId);
                    if (customItem == null)
                    {
                        LogManager.Warn($"{CustomItem.Name} - CustomItem with Id {CustomItemId} was not found for ItemShot");
                        return;
                    }

                    SummonedCustomItem summoned = new(customItem, position);
                    ApplyPhysics(ev.Player, summoned.Pickup);
                    if (summoned.Pickup is TimedGrenadeProjectile grenadePickup)
                        grenadePickup.Base.ServerActivate();
                        
                    return;
                }

                if ((ItemType == ItemType.GrenadeHE || ItemType == ItemType.GrenadeFlash || ItemType == ItemType.SCP018 || ItemType == ItemType.SCP2176) && IsGrenade)
                {
                    int fuse = ItemType == ItemType.SCP2176 ? 30 : 10;
                    Pickup? spawned = (Pickup?)TimedGrenadeProjectile.SpawnActive(position, ItemType, ev.Player, fuse);
                    if (spawned != null)
                    {
                        ApplyPhysics(ev.Player, spawned);
                        if (GrenadeExplodeOnImpact)
                        {
                            if (spawned.Base.Info.ItemId.GetItemBase() is InventorySystem.Items.ThrowableProjectiles.ThrowableItem throwableBase)
                                spawned.GameObject.AddComponent<CollisionHandler>().Init(spawned.GameObject, throwableBase.Projectile);
                        }

                        LogManager.Debug($"{CustomItem.Name} - {ItemType} spawned (ItemShot) - {spawned.Serial}");
                    }
                    return;
                }

                Pickup? pickup = Pickup.Create(ItemType, position);
                if (pickup == null)
                {
                    LogManager.Warn($"{CustomItem.Name} - Failed to create pickup for ItemType {ItemType}");
                    return;
                }

                if (pickup.Base.Info.ItemId.GetItemBase() is InventorySystem.Items.ThrowableProjectiles.ThrowableItem throwableItem)
                {
                    ThrownProjectile thrownProjectile = UnityEngine.Object.Instantiate(throwableItem.Projectile);
                    Pickup? thrownPickup = Pickup.Get(thrownProjectile);
                    if (thrownPickup != null)
                    {
                        ApplyPhysics(ev.Player, thrownPickup);

                        pickup.Base.Info.Locked = true;
                        thrownProjectile.NetworkInfo = pickup.Base.Info;
                        thrownProjectile.PreviousOwner = new Footprint(ev.Player.ReferenceHub);
                        NetworkServer.Spawn(thrownProjectile.gameObject);
                        thrownProjectile.ServerActivate();

                        if (GrenadeExplodeOnImpact)
                            pickup.GameObject.AddComponent<CollisionHandler>().Init(pickup.GameObject, throwableItem.Projectile);

                        LogManager.Debug($"{CustomItem.Name} - ThrownProjectile spawned (ItemShot) - {pickup.Serial}");
                    }
                }
                else
                {
                    ApplyPhysics(ev.Player, pickup);
                    pickup.Spawn();

                    LogManager.Debug($"{CustomItem.Name} - Pickup spawned (ItemShot) - {pickup.Serial}");
                }
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ShootingWeapon += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ShootingWeapon -= Run;
        }

        private void ApplyPhysics(Player player, Pickup? pickup)
        {
            if (pickup == null)
                return;

            float num = 1f - Mathf.Abs(Vector3.Dot(player.Camera.forward, Vector3.up));
            Vector3 forward = player.Camera.forward;
            Vector3 vector = player.Camera.up * UpwardsFactor;
            Vector3 vector3 = forward + vector * num;
            Vector3 velocityVector = vector3 * Velocity;

            Rigidbody? rb = pickup.PickupStandardPhysics?.Rb;
            if (rb != null)
            {
                rb.centerOfMass = Vector3.zero;
                rb.angularVelocity = Torque;
                rb.linearVelocity = velocityVector;
            }

            LogManager.Debug($"Applying physics to {pickup.Type} - {pickup.Serial}: VelocityVector: {velocityVector}, StartTorque: {Torque}");
        }
    }
}
