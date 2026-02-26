using Footprinting;
using InventorySystem.Items.Firearms.Extensions;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ItemShot : CustomModuleBase
    {
        public override string Name => "ItemShot";
        public override List<string> RequiredArguments => 
        [
            "IsGrenade",
            "GrenadeExplodeOnImpact",
            "IsCustomItem",
            "Velocity",
            "UpwardsFactor",
            "Torque",
            "CustomItemId",
            "ItemType"
        ];

        public bool IsGrenade { get; set; }
        public bool GrenadeExplodeOnImpact { get; set; }
        public bool IsCustomItem { get; set; }
        public float Velocity { get; set; }
        public float UpwardsFactor { get; set; }
        public Vector3 Torque { get; set; }
        public uint CustomItemId { get; set; }
        public ItemType ItemType { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<bool>("IsGrenade", out var isGrenade))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} IsGrenade is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<bool>("GrenadeExplodeOnImpact", out var grenadeExplodeOnImpact))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} GrenadeExplodeOnImpact is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<bool>("IsCustomItem", out var isCustomItem))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} IsCustomItem is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<float>("Velocity", out var velocity))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Velocity is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("UpwardsFactor", out var upwardsFactor))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} UpwardsFactor is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<Vector3>("Torque", out var torque))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Torque is not a valid Vector3!");
                    return;
                }

                if (!args.TryGetValue<uint>("CustomItemId", out var customItemId))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} CustomItemId is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<ItemType>("ItemType", out var itemType))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ItemType is not a valid Vector3!");
                    return;
                }

                IsGrenade = isGrenade;
                GrenadeExplodeOnImpact = grenadeExplodeOnImpact;
                IsCustomItem = isCustomItem;
                Velocity = velocity;
                UpwardsFactor = upwardsFactor;
                Torque = torque;
                CustomItemId = customItemId;
                ItemType = itemType;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerShootingWeaponEventArgs ev)
            {
                Vector3 position = ev.Player.Camera.position;
                if (BarrelTipExtension.TryFindWorldmodelBarrelTip(ev.FirearmItem.Serial, out var tip))
                    position = tip.WorldspacePosition;

                if (IsCustomItem)
                {
                    SummonedCustomItem summoned = new(Utilities.GetCustomItem(CustomItemId), position);
                    ApplyPhysics(ev.Player, summoned.Pickup);
                    if (summoned.Pickup is TimedGrenadeProjectile grenadePickup)
                        grenadePickup.Base.ServerActivate();
                        
                    return;
                }

                if ((ItemType == ItemType.GrenadeHE || ItemType == ItemType.GrenadeFlash || ItemType == ItemType.SCP018 || ItemType == ItemType.SCP2176) && IsGrenade)
                {
                    int fuse = ItemType == ItemType.SCP2176 ? 30 : 10;
                    Pickup spawned = (Pickup)TimedGrenadeProjectile.SpawnActive(position, ItemType, ev.Player, fuse);
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

                }

                Pickup pickup = Pickup.Create(ItemType, position);
                if (pickup == null)
                {
                    LogManager.Warn($"{CustomItem.Name} - Failed to create pickup for ItemType {ItemType}");
                    return;
                }

                if (pickup.Base.Info.ItemId.GetItemBase() is InventorySystem.Items.ThrowableProjectiles.ThrowableItem throwableItem)
                {
                    ThrownProjectile thrownProjectile = UnityEngine.Object.Instantiate(throwableItem.Projectile);
                    if (Pickup.TryGet(thrownProjectile.ItemId.SerialNumber, out var pickup1))
                    {
                        ApplyPhysics(ev.Player, Pickup.Get(thrownProjectile));

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

        private void ApplyPhysics(Player player, Pickup pickup)
        {
            float num = 1f - Mathf.Abs(Vector3.Dot(player.Camera.forward, Vector3.up));
            Vector3 forward = player.Camera.forward;
            Vector3 vector = player.Camera.up * UpwardsFactor;
            Vector3 vector3 = forward + vector * num;
            Vector3 velocityVector = vector3 * Velocity;

            Rigidbody rb = pickup.PickupStandardPhysics.Rb;
            rb.centerOfMass = Vector3.zero;
            rb.angularVelocity = Torque;
            rb.linearVelocity = velocityVector;

            LogManager.Debug($"Applying physics to {pickup.Type} - {pickup.Serial}: VelocityVector: {velocityVector}, StartTorque: {Torque}, ");
        }
    }
}
