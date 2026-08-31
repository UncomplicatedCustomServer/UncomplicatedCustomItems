using System;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Events;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Cluster : CustomModuleBase
    {
        public override string Name => "Cluster";

        public ItemType ItemToSpawn { get; set; }
        public int AmountToSpawn { get; set; }
        public float ScpDamageMultiplier { get; set; } = 1f;
        public float FuseTime { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is ProjectileExplodingEventArgs ev)
            {
                Vector3 scale = CustomItem?.Scale * 0.75f ?? Vector3.one;
                if (ItemToSpawn == ItemType.GrenadeHE || ItemToSpawn == ItemType.GrenadeFlash || ItemToSpawn == ItemType.SCP018 || ItemToSpawn == ItemType.SCP2176)
                {
                    Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                    {
                        ExplosiveGrenadeProjectile? firstgrenade = (ExplosiveGrenadeProjectile?)ExplosiveGrenadeProjectile.SpawnActive(ev.Position, ItemType.GrenadeHE, ev.Player, (double)FuseTime / 2);
                        for (int i = 0; i < AmountToSpawn; i++)
                        {
                            Vector3 position = ServerHandler.ClusterOffset(ev.Position);
                            ExplosiveGrenadeProjectile? grenade = (ExplosiveGrenadeProjectile?)ExplosiveGrenadeProjectile.SpawnActive(position, ItemToSpawn, ev.Player, (double)FuseTime);
                            if (grenade != null)
                            {
                                grenade.GameObject.transform.localScale = scale;
                                grenade.ScpDamageMultiplier = ScpDamageMultiplier;
                            }
                        }
                    });
                }
                else
                {
                    Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                    {
                        for (int i = 0; i < AmountToSpawn; i++)
                        {
                            Vector3 position = ServerHandler.ClusterOffset(ev.Position);
                            PickupExtensions.CreateAndSpawn(ItemToSpawn, position, ev.Player?.Rotation ?? Quaternion.identity, scale);
                        }
                    });
                }
            }
        }

        public override void RegisterEvents()
        {
            ServerEvents.ProjectileExploding += Run;
        }

        public override void UnregisterEvents()
        {
            ServerEvents.ProjectileExploding -= Run;
        }
    }
}