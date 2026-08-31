using LabApi.Events.Arguments.ServerEvents;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UnityEngine;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;
using UncomplicatedCustomItems.API.Features.Networking;

namespace UncomplicatedCustomItems.Events
{
    internal class ServerHandler
    {
        private static readonly System.Random _clusterRandom = new();

        public static void Register()
        {
            ServerEvent.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvent.PickupDestroyed += OnPickup;
            ServerEvent.ProjectileExploding += OnGrenadeExploding;
            ServerEvent.RoundStarted += SpawnItemsOnRoundStarted;
            ServerEvent.ProjectileExploded += OnDetonated;
        }

        public static void Unregister()
        {
            ServerEvent.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvent.PickupDestroyed -= OnPickup;
            ServerEvent.ProjectileExploding -= OnGrenadeExploding;
            ServerEvent.RoundStarted -= SpawnItemsOnRoundStarted;
            ServerEvent.ProjectileExploded -= OnDetonated;
        }

        private static void OnWaitingForPlayers()
        {
            SummonedCustomItem.Cleanup();
            SummonedCustomItem.CleanupCooldownStates();
            SummonedAPICustomItem.Cleanup();
            APIRequest.Cleanup();
            PlayerHandler._capybaras.Clear();
            PlayerHandler._damageTimes.Clear();
            PlayerHandler._toolGunPrimitives.Clear();
            PlayerHandler.Appearance.Clear();
            PlayerHandler._humeShieldRegenCoroutine.Clear();
            PlayerHandler.ActiveLights.Clear();
            PlayerExtensions.PlayerKills.Clear();
            PlayerHandler.CustomScp268Effects.Clear();
            PlayerHandler.CandyIdx.Clear();
            CustomCandy.Candyidx.Clear();

            HarmonyElements.Patches.LockerSpawningItemPrefix.Reset();

            Capybara.capybaras.Clear();
            Disguise.Appearance.Clear();
        }

        private static void OnDetonated(ProjectileExplodedEventArgs ev)
        {
            if (ev.TimedGrenade == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.TimedGrenade.Serial, out var item) && item != null)
            {
                item.OnDetonated(ev);
            }

            if (SummonedAPICustomItem.TryGet(ev.TimedGrenade.Serial, out var api) && api != null)
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
                if (customItem.Item is ItemType.SCP330 && customItem.CustomData is CandyData data && !data.AllowSpawningAsItem)
                    continue;

                LogManager.Debug($"{customItem.Name} DoSpawn is set to {customItem.Spawn?.DoSpawn}");
                if (customItem.Spawn != null && customItem.Spawn.DoSpawn)
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
                            if (Random.Range(0f, 100f) < item.ChanceToSpawn)
                            {
                                LogManager.Debug($"Spawning {item.Name} ({count + 1}/{item.AmountToSpawn})");
                                APICustomItem.SummonItem(item, ignoreChance: true);
                            }
                        }
                    }
                }
            }
        }

        public static void OnGrenadeExploding(ProjectileExplodingEventArgs ev)
        {
            if (ev.TimedGrenade == null || ev.Player == null)
                return;

            PlayerHandler.DetonationPosition = ev.Position;

            if (!Utilities.TryGetSummonedCustomItem(ev.TimedGrenade.Serial, out SummonedCustomItem? customItem) || customItem == null)
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
            lock (_clusterRandom)
            {
                float x = position.x - 1f + (float)_clusterRandom.NextDouble() * 3f;
                float y = position.y;
                float z = position.z - 1f + (float)_clusterRandom.NextDouble() * 3f;
                return new Vector3(x, y, z);
            }
        }
    }
}