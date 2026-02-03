using LabApi.Events.Arguments.ServerEvents;
using System;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UnityEngine;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;

namespace UncomplicatedCustomItems.Events
{
    internal class ServerHandler
    {
        public static void Register()
        {
            ServerEvent.PickupDestroyed += OnPickup;
            ServerEvent.ProjectileExploding += OnGrenadeExploding;
            ServerEvent.RoundStarted += SpawnItemsOnRoundStarted;
            ServerEvent.ProjectileExploded += OnDetonated;
        }

        public static void Unregister()
        {
            ServerEvent.PickupDestroyed -= OnPickup;
            ServerEvent.ProjectileExploding -= OnGrenadeExploding;
            ServerEvent.RoundStarted -= SpawnItemsOnRoundStarted;
            ServerEvent.ProjectileExploded -= OnDetonated;
        }

        private static void OnDetonated(ProjectileExplodedEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.TimedGrenade.Serial, out var item))
            {
                item.OnDetonated(ev);
            }

            if (SummonedAPICustomItem.TryGet(ev.TimedGrenade.Serial, out var api))
            {
                api.OnDetonated(ev);
            }
        }

        /// <summary>
        /// Spawn items on round started
        /// </summary>
        public static void SpawnItemsOnRoundStarted()
        {
            foreach (ICustomItem customItem in CustomItem.List)
            {
                if (customItem.Item is ItemType.SCP330 && customItem.CustomData is ICandyData data && !data.AllowSpawningAsItem)
                    continue;

                LogManager.Debug($"{customItem.Name} DoSpawn is set to {customItem.Spawn.DoSpawn}");
                if (customItem.Spawn is not null && customItem.Spawn.DoSpawn)
                {
                    for (uint count = 0; count < customItem.Spawn.Count; count++)
                    {
                        LogManager.Debug($"Spawning {customItem.Name} ({count + 1}/{customItem.Spawn.Count})");
                        Utilities.SummonCustomItem(customItem);
                    }
                }
            }

            if (APICustomItem.List.Count > 0)
            {
                foreach (APICustomItem item in APICustomItem.List)
                {
                    if (item is CustomCandy data && !data.AllowSpawningAsItem)
                        continue;

                    LogManager.Debug($"{item.Name} DoSpawn is set to {item.Spawn}");
                    if (item.Spawn)
                    {
                        for (uint count = 0; count < item.AmountToSpawn; count++)
                        {
                            float chance = UnityEngine.Random.Range(0f, 101f);
                            if (chance >= item.ChanceToSpawn)
                            {
                                LogManager.Debug($"Spawning {item.Name} ({count + 1}/{item.AmountToSpawn})");
                                APICustomItem.SummonItem(item);
                            }
                        }
                    }
                }
            }
        }

        public static void OnGrenadeExploding(ProjectileExplodingEventArgs ev)
        {
            if (ev.TimedGrenade == null || ev.Player == null || ev.Position == null)
                return;

            PlayerHandler.DetonationPosition = ev.Position;

            if (!Utilities.TryGetSummonedCustomItem(ev.TimedGrenade.Serial, out SummonedCustomItem customItem))
                return;

            if (customItem.CustomItem.CustomItemType is CustomItemType.Item)
                customItem.HandleEvent(ev.Player, ItemEvents.Detonation, ev.TimedGrenade.Serial);
        }

        public static void OnPickup(PickupDestroyedEventArgs ev)
        {
            if (ev.Pickup != null)
                PlayerHandler.DestroyLightOnPickup(ev.Pickup);
        }

        internal static Vector3 ClusterOffset(Vector3 position)
        {
            System.Random random = new();
            float x = position.x - 1 + ((float)random.NextDouble() * random.Next(0, 3));
            float y = position.y;
            float z = position.z - 1 + ((float)random.NextDouble() * random.Next(0, 3));
            return new Vector3(x, y, z);
        }
    }
}