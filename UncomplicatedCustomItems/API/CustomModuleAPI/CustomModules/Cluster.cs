using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Events;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Cluster : CustomModuleBase
    {
        public override string Name => "Cluster";
        public override List<string> RequiredArguments => 
        [
            "ItemToSpawn",
            "AmountToSpawn",
            "ScpDamageMultiplier",
            "FuseTime"
        ];

        public ItemType ItemToSpawn { get; set; }
        public int? AmountToSpawn { get; set; }
        public float ScpDamageMultiplier { get; set; }
        public float FuseTime { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<ItemType>("ItemToSpawn", out var itemToSpawn))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ItemToSpawn is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(ItemType)))}");
                    return;
                }

                if (!args.TryGetValue<int>("AmountToSpawn", out var amountToSpawn))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} AmountToSpawn is not a valid int!");
                    return;
                }

                if (!args.TryGetValue<float>("ScpDamageMultiplier", out var scpDamageMultiplier))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ScpDamageMultiplier is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("FuseTime", out var fuseTime))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} FuseTime is not a valid float!");
                    return;
                }

                ItemToSpawn = itemToSpawn;
                AmountToSpawn = amountToSpawn;
                ScpDamageMultiplier = scpDamageMultiplier;
                FuseTime = fuseTime;
            }
        }

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
                        for (int i = 0; i <= AmountToSpawn; i++)
                        {
                            Vector3 position = ServerHandler.ClusterOffset(ev.Position);
                            ExplosiveGrenadeProjectile? grenade = (ExplosiveGrenadeProjectile?)ExplosiveGrenadeProjectile.SpawnActive(position, ItemToSpawn, ev.Player, (double)FuseTime);
                            grenade?.GameObject.transform.localScale = scale;
                            grenade?.ScpDamageMultiplier = ScpDamageMultiplier;
                        }
                    });
                }
                else
                {
                    Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                    {
                        for (int i = 0; i <= AmountToSpawn; i++)
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