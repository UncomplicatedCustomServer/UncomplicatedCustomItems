using Exiled.Events.EventArgs.Map;
using InventorySystem.Items.Usables.Scp244;
using MEC;
using System.Collections.Generic;
using LabApiWrappers = LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API;
using UnityEngine;
using Exiled.API.Features.Pickups;
using System;
using Light = Exiled.API.Features.Toys.Light;
using Mirror;
using UncomplicatedCustomItems.API.Extensions;

namespace UncomplicatedCustomItems.Events
{
    internal class MapHandler
    {
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public static Dictionary<Pickup, Light> ActiveLights = [];

        /// <summary>
        /// The <see cref="Vector3"/> coordinates of the latest detonation point for a <see cref="Exiled.API.Features.Pickups.Projectiles.EffectGrenadeProjectile"/>.
        /// Triggered by the <see cref="GrenadeExploding"/> method.
        /// </summary>
        public static Vector3 DetonationPosition { get; set; } = Vector3.zero;

        public static void Register()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += GrenadeExploding;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= GrenadeExploding;
        }
        public static void GrenadeExploding(ExplodingGrenadeEventArgs ev)
        {
            if (ev.Projectile == null || ev.Player == null || ev.Position == null)
                return;
            DetonationPosition = ev.Position;
            if (!Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;

            LogManager.Debug($"{ev.Projectile.Type} is a CustomItem");
            if (CustomItem.HasModule(CustomFlags.SpawnItemWhenDetonated))
            {
                foreach (SpawnItemWhenDetonatedSettings SpawnItemWhenDetonatedSettings in CustomItem.CustomItem.FlagSettings.SpawnItemWhenDetonatedSettings)
                {
                    if (SpawnItemWhenDetonatedSettings.Chance == null || SpawnItemWhenDetonatedSettings.ItemId == null || SpawnItemWhenDetonatedSettings.ItemType == null || SpawnItemWhenDetonatedSettings.Pickupable == null || SpawnItemWhenDetonatedSettings.TimeTillDespawn == null)
                    {
                        LogManager.Warn($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} Chance, ItemId, ItemType, Pickupable, or TimeTillDespawn equals null. Aborting... \n Values: {SpawnItemWhenDetonatedSettings.Chance} {SpawnItemWhenDetonatedSettings.ItemId} {SpawnItemWhenDetonatedSettings.ItemType} {SpawnItemWhenDetonatedSettings.Pickupable} {SpawnItemWhenDetonatedSettings.TimeTillDespawn}");
                        break;
                    }

                    int Chance = UnityEngine.Random.Range(0, 100);
                    if (Chance <= SpawnItemWhenDetonatedSettings.Chance)
                    {
                        LogManager.Debug($"Loaded FlagSettings.");
                        if (SpawnItemWhenDetonatedSettings.ItemType == "UCI" || SpawnItemWhenDetonatedSettings.ItemType == "uci")
                        {
                            if (Utilities.TryGetCustomItem((uint)SpawnItemWhenDetonatedSettings.ItemId, out ICustomItem customItem))
                            {
                                SummonedCustomItem customitem = new(customItem, ev.Position);
                                if (SpawnItemWhenDetonatedSettings.Pickupable == false)
                                {
                                    customitem.Pickup.Weight = 5000f;
                                }
                                if (SpawnItemWhenDetonatedSettings.TimeTillDespawn != null || SpawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(customitem.Serial, (float)SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                            else
                                LogManager.Warn($"{SpawnItemWhenDetonatedSettings.ItemId} is not a UCI CustomItem ID!");
                        }
                        else if (SpawnItemWhenDetonatedSettings.ItemType == "ECI" || SpawnItemWhenDetonatedSettings.ItemType == "eci")
                        {
                            if (Exiled.CustomItems.API.Features.CustomItem.TryGet((uint)SpawnItemWhenDetonatedSettings.ItemId, out Exiled.CustomItems.API.Features.CustomItem ExCustomItem))
                            {
                                Pickup exCustomItem = ExCustomItem.Spawn(ev.Position);
                                if (SpawnItemWhenDetonatedSettings.Pickupable == false)
                                    exCustomItem.Weight = 5000f;
                                if (SpawnItemWhenDetonatedSettings.TimeTillDespawn != null || SpawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(exCustomItem.Serial, (float)SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                            else
                                LogManager.Warn($"{SpawnItemWhenDetonatedSettings.ItemId} is not a Exiled CustomItem ID!");
                        }
                        else if (SpawnItemWhenDetonatedSettings.ItemType == "Normal" || SpawnItemWhenDetonatedSettings.ItemType == "normal")
                        {
                            if ((ItemType)SpawnItemWhenDetonatedSettings.ItemId == ItemType.SCP244a || (ItemType)SpawnItemWhenDetonatedSettings.ItemId == ItemType.SCP244b)
                            {
                                LogManager.Debug($"Item is SCP244a or SCP244b");
                                LabApiWrappers.Scp244Pickup scp244Pickup = (LabApiWrappers.Scp244Pickup)LabApiWrappers.Scp244Pickup.Create((ItemType)SpawnItemWhenDetonatedSettings.ItemId, ev.Position);
                                scp244Pickup.Base.MaxDiameter = 0.1f;
                                scp244Pickup.State = Scp244State.Active;
                                scp244Pickup.Spawn();

                                if (SpawnItemWhenDetonatedSettings.Pickupable == false)
                                    scp244Pickup.Weight = 5000f;
                                if (SpawnItemWhenDetonatedSettings.TimeTillDespawn != null || SpawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(scp244Pickup.Serial, (float)SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                            else
                            {
                                LabApiWrappers.Pickup pickup = LabApiWrappers.Pickup.Create((ItemType)SpawnItemWhenDetonatedSettings.ItemId, ev.Position);
                                Vector3 vector3 = new(0f, 1f, 0f);
                                pickup.Transform.position = pickup.Transform.position + vector3;
                                pickup.Spawn();

                                if (SpawnItemWhenDetonatedSettings.Pickupable == false)
                                    pickup.Weight = 5000f;
                                if (SpawnItemWhenDetonatedSettings.TimeTillDespawn != null || SpawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                                {
                                    LogManager.Debug($"Starting Despawn Coroutine");
                                    Timing.RunCoroutine(TimeTillDespawnCoroutine(pickup.Serial, (float)SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                LogManager.Debug($"{ev.Projectile.Type} is not a CustomItem with the SpawnItemWhenDetonated flag. Serial: {ev.Projectile.Serial}");
            }
            if (CustomItem.HasModule(CustomFlags.Cluster))
            {
                LogManager.Debug($"{ev.Projectile.Type} is a CustomItem");
                foreach (ClusterSettings ClusterSettings in CustomItem.CustomItem.FlagSettings.ClusterSettings)
                {
                    Vector3 Scale = CustomItem.CustomItem.Scale * 0.75f;
                    if (ClusterSettings.ItemToSpawn == ItemType.GrenadeHE || ClusterSettings.ItemToSpawn == ItemType.GrenadeFlash || ClusterSettings.ItemToSpawn == ItemType.SCP018)
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            for (int i = 0; i <= ClusterSettings.AmountToSpawn; i++)
                            {
                                Vector3 position = ClusterOffset(ev.Position);
                                LabApiWrappers.ExplosiveGrenadeProjectile grenade = (LabApiWrappers.ExplosiveGrenadeProjectile)LabApiWrappers.ExplosiveGrenadeProjectile.SpawnActive(position, ClusterSettings.ItemToSpawn, ev.Player, (double)ClusterSettings.FuseTime);
                                grenade.GameObject.transform.localScale = Scale;
                                grenade.ScpDamageMultiplier = ClusterSettings.ScpDamageMultiplier ?? 1f;
                            }
                        });
                    }
                    else
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            for (int i = 0; i <= ClusterSettings.AmountToSpawn; i++)
                            {
                                Vector3 position = ClusterOffset(ev.Position);
                                LabApiWrappers.Pickup pickup = LabApiWrappers.Pickup.Create(ClusterSettings.ItemToSpawn, position, ev.Player.Rotation, Scale);
                                pickup.Spawn();
                            }
                        });
                    }
                }
            }
            else
            {
                LogManager.Debug($"{ev.Projectile.Type} is not a CustomItem with the Cluster flag. Serial: {ev.Projectile.Serial}");
            }
        }

        public static void OnPickupCreation(PickupAddedEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem SummonedCustomItem))
            {
                Timing.CallDelayed(1f, () =>
                {
                    try
                    {
                        ev.Pickup.Scale = SummonedCustomItem.CustomItem.Scale;
                        ev.Pickup.Weight = SummonedCustomItem.CustomItem.Weight;
                    }
                    catch (Exception ex)
                    {
                        LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                        LogManager.Error($"Couldnt set CustomItem Pickup Scale or CustomItem Pickup Weight\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                    }
                });
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.ItemGlow))
            {
                Timing.CallDelayed(1f, () =>
                {
                    foreach (ItemGlowSettings ItemGlowSettings in customItem.CustomItem.FlagSettings.ItemGlowSettings)
                    {
                        LogManager.Debug("SpawnLightOnItem method triggered");

                        if (ev.Pickup?.Base?.gameObject == null)
                            return;

                        GameObject itemGameObject = ev.Pickup.Base.gameObject;
                        Color lightColor = Color.blue;

                        if (ItemGlowSettings != null)
                        {
                            if (!string.IsNullOrEmpty(ItemGlowSettings.GlowColor))
                            {
                                if (ColorUtility.TryParseHtmlString(ItemGlowSettings.GlowColor, out Color parsedColor))
                                {
                                    lightColor = parsedColor;
                                }
                                else
                                {
                                    LogManager.Error($"Failed to parse color: {ItemGlowSettings.GlowColor} for {customItem.CustomItem.Name}");
                                }
                            }
                        }
                        else
                        {
                            LogManager.Error("No FlagSettings found on custom item");
                        }

                        var light = Light.Create(ev.Pickup.Position);
                        light.Color = lightColor;
                        light.Intensity = 0.7f;
                        light.Range = 0.5f;
                        light.ShadowType = LightShadows.None;

                        light.Base.gameObject.transform.SetParent(itemGameObject.transform, true);
                        LogManager.Debug($"Item Light spawned at position: {light.Base.transform.position}");
                        ActiveLights[ev.Pickup] = light;
                    }
                });
            }
        }

        public static void OnPickup(PickupDestroyedEventArgs ev)
        {
            if (ev.Pickup != null)
            {
                if (ev.Pickup != null)
                {
                    DestroyLightOnPickup(ev.Pickup);
                }
                else
                {
                    LogManager.Error($"Couldnt destroy light on {ev.Pickup.Type}.");
                }
            }
        }

        /// <summary>
        /// A coroutine that destroys a pickup by its serial after a set amount of time.
        /// </summary>
        public static IEnumerator<float> TimeTillDespawnCoroutine(ushort Serial, float DespawnTime)
        {
            yield return Timing.WaitForSeconds(DespawnTime);
            Pickup Pickup = Pickup.Get(Serial);
            if (Pickup != null)
            {
                Pickup.Destroy();
                LogManager.Debug($"Destroyed pickup. Type: {Pickup.Type} Previous owner: {Pickup.PreviousOwner} Serial: {Pickup.Serial}");
            }
        }
        private static Vector3 ClusterOffset(Vector3 position)
        {
            System.Random random = new System.Random();
            float x = position.x - 1 + ((float)random.NextDouble() * random.Next(0, 3));
            float y = position.y;
            float z = position.z - 1 + ((float)random.NextDouble() * random.Next(0, 3));
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// Destroys the <see cref="Light"/> on a <see cref="CustomItem"/> <see cref="Pickup"/>.
        /// <param name="Pickup"></param>
        /// </summary>
        public static void DestroyLightOnPickup(Pickup Pickup)
        {
            if (Utilities.IsSummonedCustomItem(Pickup.Serial))
            {
                LogManager.Debug($"{Pickup.Type} is a Customitem");
                if (Pickup == null || !ActiveLights.ContainsKey(Pickup))
                    return;
                Light ItemLight = ActiveLights[Pickup];
                if (ItemLight != null && ItemLight.Base != null)
                {
                    NetworkServer.Destroy(ItemLight.Base.gameObject);
                    LogManager.Debug($"Destroyed light on {Pickup.Type}");
                }
                ActiveLights.TryRemove(Pickup);
                LogManager.Debug("Light successfully destroyed.");
            }
            else
            {
                return;
            }
        }
    }
}
