using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp244;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.Commands;
using UnityEngine;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;

namespace UncomplicatedCustomItems.Events
{
    internal class ServerHandler
    {
        public static void Register()
        {
            ServerEvent.PickupDestroyed += OnPickup;
            ServerEvent.ProjectileExploding += OnGrenadeExploding;
            ServerEvent.RoundEnding += OnRoundEnd;
            ServerEvent.PickupCreated += OnPickupCreation;
            ServerEvent.RoundStarted += SpawnItemsOnRoundStarted;
            ServerEvent.ProjectileExploded += OnDetonated;
        }

        public static void Unregister()
        {
            ServerEvent.PickupDestroyed -= OnPickup;
            ServerEvent.ProjectileExploding -= OnGrenadeExploding;
            ServerEvent.RoundEnding -= OnRoundEnd;
            ServerEvent.PickupCreated -= OnPickupCreation;
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

        public static void OnRoundEnd(RoundEndingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            CustomItem.List.Clear();
            CustomItem.UnregisteredCustomItems.Clear();
            CustomItem.CustomItems.Clear();
            CustomItem.UnregisteredCustomItems.Clear();
            CustomAction.CustomActions.Clear();
            CustomAction.List.Clear();
            CustomAction.UnregisteredCustomActions.Clear();
            CustomAction.UnregisteredList.Clear();
            SummonedCustomItem.List.ForEach(sci => sci.Destroy());
            SummonedAPICustomItem.List.ForEach(sci => sci.Destroy());
            ArgumentManager._actionHandlers.Clear();
            ArgumentManager._eventArgPropertyCache.Clear();
            BaseCommand.Subcommands.Clear();
            PlayerHandler._capybaras.Clear();
            PlayerHandler._damageTimes.Clear();
            PlayerHandler._toolGunPrimitives.Clear();
            PlayerHandler.CustomScp268Effects.Clear();
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

            LogManager.Debug($"{ev.TimedGrenade.Type} is a CustomItem");
            if (customItem.HasModule(CustomFlags.SpawnItemWhenDetonated))
            {
                foreach (SpawnItemWhenDetonatedSettings spawnItemWhenDetonatedSettings in customItem.CustomItem.FlagSettings.SpawnItemWhenDetonatedSettings)
                {
                    if (spawnItemWhenDetonatedSettings.Chance == null || spawnItemWhenDetonatedSettings.ItemId == null || spawnItemWhenDetonatedSettings.ItemType == null || spawnItemWhenDetonatedSettings.Pickupable == null || spawnItemWhenDetonatedSettings.TimeTillDespawn == null)
                    {
                        LogManager.Warn($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} Chance, ItemId, ItemType, Pickupable, or TimeTillDespawn equals null. Aborting... \n Values: {spawnItemWhenDetonatedSettings.Chance} {spawnItemWhenDetonatedSettings.ItemId} {spawnItemWhenDetonatedSettings.ItemType} {spawnItemWhenDetonatedSettings.Pickupable} {spawnItemWhenDetonatedSettings.TimeTillDespawn}");
                        continue;
                    }
                    
                    float chance = UnityEngine.Random.Range(0f, 101f);
                    if (chance >= spawnItemWhenDetonatedSettings.Chance)
                    {
                        LogManager.Debug($"Loaded FlagSettings.");
                        if (spawnItemWhenDetonatedSettings.ItemType.ToLower() == "uci")
                        {
                            if (Utilities.TryGetCustomItem((uint)spawnItemWhenDetonatedSettings.ItemId, out ICustomItem itemToSpawn))
                            {
                                SummonedCustomItem summonedItem = new(itemToSpawn, ev.Position);
                                if (spawnItemWhenDetonatedSettings.Pickupable == false)
                                {
                                    summonedItem.Pickup.Weight = 5000f;
                                }
                                if (spawnItemWhenDetonatedSettings.TimeTillDespawn != null || spawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(summonedItem.Serial, (float)spawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                            else
                                LogManager.Warn($"{spawnItemWhenDetonatedSettings.ItemId} is not a UCI CustomItem ID!");
                        }
#if EXILED
                        else if (spawnItemWhenDetonatedSettings.ItemType == "ECI" || spawnItemWhenDetonatedSettings.ItemType == "eci")
                        {
                            if (Exiled.CustomItems.API.Features.CustomItem.TryGet((uint)spawnItemWhenDetonatedSettings.ItemId, out Exiled.CustomItems.API.Features.CustomItem ExCustomItem))
                            {
                                Exiled.API.Features.Pickups.Pickup exCustomItem = ExCustomItem.Spawn(ev.Position);
                                if (spawnItemWhenDetonatedSettings.Pickupable == false)
                                    exCustomItem.Weight = 5000f;
                                if (spawnItemWhenDetonatedSettings.TimeTillDespawn != null || spawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(exCustomItem.Serial, (float)spawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                            else
                                LogManager.Warn($"{spawnItemWhenDetonatedSettings.ItemId} is not a Exiled CustomItem ID!");
                        }
#endif
                        else if (spawnItemWhenDetonatedSettings.ItemType.ToLower() == "normal")
                        {
                            if ((ItemType)spawnItemWhenDetonatedSettings.ItemId == ItemType.SCP244a || (ItemType)spawnItemWhenDetonatedSettings.ItemId == ItemType.SCP244b)
                            {
                                LogManager.Debug($"Item is SCP244a or SCP244b");
                                Scp244Pickup scp244Pickup = (Scp244Pickup)Scp244Pickup.Create((ItemType)spawnItemWhenDetonatedSettings.ItemId, ev.Position);
                                scp244Pickup.Base.MaxDiameter = 0.1f;
                                scp244Pickup.State = Scp244State.Active;
                                scp244Pickup.Spawn();
                                if (spawnItemWhenDetonatedSettings.Pickupable == false)
                                    scp244Pickup.Weight = 5000f;
                                if (spawnItemWhenDetonatedSettings.TimeTillDespawn != null || spawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(scp244Pickup.Serial, (float)spawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                            else
                            {
                                Pickup pickup = Pickup.Create((ItemType)spawnItemWhenDetonatedSettings.ItemId, ev.Position);
                                Vector3 vector3 = new(0f, 1f, 0f);
                                pickup.Transform.position = pickup.Transform.position + vector3;
                                pickup.Spawn();
                                if (spawnItemWhenDetonatedSettings.Pickupable == false)
                                    pickup.Weight = 5000f;
                                if (spawnItemWhenDetonatedSettings.TimeTillDespawn != null || spawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(pickup.Serial, (float)spawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                        }

                    }
                }
            }
            else
                LogManager.Debug($"{ev.TimedGrenade.Type} is not a CustomItem with the SpawnItemWhenDetonated flag. Serial: {ev.TimedGrenade.Serial}");

            if (customItem.HasModule(CustomFlags.Cluster))
            {
                LogManager.Debug($"{ev.TimedGrenade.Type} is a CustomItem");
                foreach (ClusterSettings clusterSettings in customItem.CustomItem.FlagSettings.ClusterSettings)
                {
                    Vector3 scale = customItem.CustomItem.Scale * 0.75f;
                    if (clusterSettings.ItemToSpawn == ItemType.GrenadeHE || clusterSettings.ItemToSpawn == ItemType.GrenadeFlash || clusterSettings.ItemToSpawn == ItemType.SCP018)
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            ExplosiveGrenadeProjectile firstgrenade = (ExplosiveGrenadeProjectile)ExplosiveGrenadeProjectile.SpawnActive(ev.Position, ItemType.GrenadeHE, ev.Player, (double)clusterSettings.FuseTime / 2);
                            for (int i = 0; i <= clusterSettings.AmountToSpawn; i++)
                            {
                                Vector3 position = ClusterOffset(ev.Position);
                                ExplosiveGrenadeProjectile grenade = (ExplosiveGrenadeProjectile)ExplosiveGrenadeProjectile.SpawnActive(position, clusterSettings.ItemToSpawn, ev.Player, (double)clusterSettings.FuseTime);
                                grenade.GameObject.transform.localScale = scale;
                                grenade.ScpDamageMultiplier = clusterSettings.ScpDamageMultiplier ?? 1f;
                            }
                        });
                    }
                    else
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            for (int i = 0; i <= clusterSettings.AmountToSpawn; i++)
                            {
                                Vector3 position = ClusterOffset(ev.Position);
                                PickupExtensions.CreateAndSpawn(clusterSettings.ItemToSpawn, position, ev.Player.Rotation, scale);
                            }
                        });
                    }
                }
            }
        }

        public static void OnPickupCreation(PickupCreatedEventArgs ev)
        {
            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem customItem))
                return;

            customItem?.OnDrop(ev);
            Timing.CallDelayed(Timing.WaitForOneFrame, () =>
            {
                try
                {
                    NetworkServer.UnSpawn(ev.Pickup.GameObject);
                    ev.Pickup.GameObject.transform.localScale = customItem.CustomItem.Scale;
                    ev.Pickup.Weight = customItem.CustomItem.Weight;
                    ev.Pickup.Spawn();
                }
                catch (Exception ex)
                {
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Error($"Couldnt set CustomItem Pickup Scale or CustomItem Pickup Weight\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                }

                if (customItem.HasModule(CustomFlags.ToolGun))
                    customItem.Destroy();

                if (customItem.HasModule(CustomFlags.ItemGlow))
                {
                    foreach (ItemGlowSettings itemGlowSettings in customItem.CustomItem.FlagSettings.ItemGlowSettings)
                    {
                        LogManager.Debug("SpawnLightOnItem method triggered");

                        if (ev.Pickup?.Base?.gameObject == null)
                            return;

                        GameObject itemGameObject = ev.Pickup.Base.gameObject;
                        Color lightColor = Color.blue;

                        if (itemGlowSettings != null)
                        {
                            if (!string.IsNullOrEmpty(itemGlowSettings.GlowColor))
                            {
                                if (ColorUtility.TryParseHtmlString(itemGlowSettings.GlowColor, out Color parsedColor))
                                {
                                    lightColor = parsedColor;
                                }
                                else
                                {
                                    LogManager.Error($"Failed to parse color: {itemGlowSettings.GlowColor} for {customItem.CustomItem.Name}");
                                }
                            }
                        }
                        else
                        {
                            LogManager.Error("No FlagSettings found on custom item");
                        }

                        var light = Light.Create(ev.Pickup.Position);
                        light.Color = lightColor;
                        light.Intensity = itemGlowSettings.Intensity;
                        light.Range = itemGlowSettings.Range;
                        light.ShadowType = LightShadows.None;

                        light.Base.gameObject.transform.SetParent(itemGameObject.transform, true);
                        light.Position += Vector3.up * 0.1f;
                        LogManager.Debug($"Item Light spawned at position: {light.Position}");
                        PlayerHandler.ActiveLights[ev.Pickup] = light;
                    }
                }
            });
        }

        public static void OnPickup(PickupDestroyedEventArgs ev)
        {
            if (ev.Pickup != null)
                PlayerHandler.DestroyLightOnPickup(ev.Pickup);
        }

        /// <summary>
        /// A coroutine that destroys a pickup by its serial after a set amount of time.
        /// </summary>
        public static IEnumerator<float> TimeTillDespawnCoroutine(ushort serial, float despawnTime)
        {
            yield return Timing.WaitForSeconds(despawnTime);
            Pickup pickup = Pickup.Get(serial);
            if (pickup != null)
            {
                pickup.Destroy();
                LogManager.Debug($"Destroyed pickup. Type: {pickup.Type} Previous owner: {pickup.LastOwner} Serial: {pickup.Serial}");
            }
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