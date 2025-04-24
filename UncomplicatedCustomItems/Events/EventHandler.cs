using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomModules;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Light = Exiled.API.Features.Toys.Light;
using Mirror;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using CustomPlayerEffects;
using Exiled.API.Features.Toys;
using InventorySystem.Items.Usables.Scp244;
using MEC;
using Exiled.CustomRoles.API.Features;
using UncomplicatedCustomItems.Integrations;
using PlayerRoles;
using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        /// <summary>
        /// The Dictionary that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public Dictionary<Pickup, Light> ActiveLights = [];
        public Vector3 DetonationPosition { get; set; }
        public void OnHurt(HurtEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker.CurrentItem == null)
                return;

            LogManager.Debug("OnHurt event is being triggered");
            if (ev.Player is not null && ev.Attacker is not null && Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem summonedCustomItem))
            {
                foreach (LifeStealSettings LifeStealSettings in summonedCustomItem.CustomItem.FlagSettings.LifeStealSettings)
                {
                    if (Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<LifeSteal>())
                    {
                        LogManager.Debug("LifeSteal custom flag is being triggered");

                        if (LifeStealSettings != null)
                        {
                            float HealedAmount = LifeStealSettings.LifeStealAmount * LifeStealSettings.LifeStealPercentage;
                            ev.Attacker.Heal(HealedAmount);
                            LogManager.Debug($"LifeSteal custom flag triggered, healed {HealedAmount} HP");
                        }
                    }
                }
            }
        }
        public void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Player == null || ev.Player.CurrentItem == null)
                return;

            if (ev.Player is not null && Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) && customItem.HasModule<DoNotTriggerTeslaGates>())
                ev.IsTriggerable = false;
            else return;
        }

        public void OnShooting(ShootingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) && customItem.HasModule<InfiniteAmmo>())
            {
                if (ev.Firearm != null)
                {
                    if (ev.Firearm is Firearm Firearm)
                    {
                        Firearm.MagazineAmmo = Firearm.MaxMagazineAmmo;
                        LogManager.Debug($"InfiniteAmmo flag was triggered: magazine refilled to {Firearm.MagazineAmmo}"); // This will spam the console if debug is enabled and a customitem has the infinite ammo flag.
                    }
                }
                else
                {
                    LogManager.Error("InfiniteAmmo flag was triggered but no valid firearm found.");
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<CustomSound>())
            {
                AudioApi AudioApi = new();
                if (ev.Firearm != null)
                {
                    LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {CustomItem.CustomItem.Name}.");
                    AudioApi.PlayAudio(CustomItem, ev.Player.Position);
                }
            }
            else return;
        }
        public void OnDieOnUseFlag(ShootingEventArgs ev)
        {
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<DieOnUse>())
            {
                if (ev.Item != null)
                {
                    ev.Player.Kill($"Killed by {CustomItem.CustomItem.Name}");
                    LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} killed.");
                }
                else
                {
                    LogManager.Error($"DieOnUse flag was triggered but couldnt be ran for {CustomItem.CustomItem.Name}.");
                }
            }
            else return;
        }
        public void OnItemUse(UsingItemCompletedEventArgs ev)
        {
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) && customItem.HasModule<DieOnUse>())
            {
                if (ev.Item != null)
                {
                    ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                    LogManager.Debug("DieOnUse triggered: user killed.");
                }
                else
                {
                    LogManager.Error($"DieOnUse flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<EffectWhenUsed>())
            {
                foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Item != null)
                    {

                        if (EffectSettings.EffectEvent != null)
                        {

                            if (EffectSettings.EffectEvent == "EffectWhenUsed")
                            {
                                if (EffectSettings.Effect == null)
                                {
                                    LogManager.Warn($"Invalid Effect: {EffectSettings.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectDuration < -1)
                                {
                                    LogManager.Warn($"Invalid Duration: {EffectSettings.EffectDuration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {EffectSettings.EffectIntensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }

                                LogManager.Debug($"Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Player}");
                                EffectType Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player.EnableEffect(Effect, Intensity, Duration, true);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectWhenUsed Flag was triggered but couldnt be ran for {CustomItem.CustomItem.Name}");
                    }
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Customitem) && Customitem.HasModule<CustomSound>())
            {
                AudioApi AudioApi = new();
                if (ev.Item != null)
                {
                    LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {Customitem.CustomItem.Name}.");
                    AudioApi.PlayAudio(Customitem, ev.Player.Position);
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customitem))
            {
                ISCP500Data SCP500Data = customitem.CustomItem.CustomData as ISCP500Data;
                ISCP207Data SCP207Data = customitem.CustomItem.CustomData as ISCP207Data;
                ISCP1853Data SCP1853Data = customitem.CustomItem.CustomData as ISCP1853Data;
                ISCP1576Data SCP1576Data = customitem.CustomItem.CustomData as ISCP1576Data;
                if (ev.Item.Type == ItemType.SCP500)
                {
                    if (SCP500Data.Effect == null)
                    {
                        LogManager.Warn($"Invalid Effect: {SCP500Data.Effect} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP500Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP500Data.Duration} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP500Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP500Data.Intensity} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }

                    LogManager.Debug($"Applying effect {SCP500Data.Effect} at intensity {SCP500Data.Intensity}, duration is {SCP500Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP500Data.Effect;
                    float Duration = SCP500Data.Duration;
                    byte Intensity = SCP500Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP207 || ev.Item.Type == ItemType.AntiSCP207)
                {
                    if (SCP207Data.Effect == null)
                    {
                        LogManager.Warn($"Invalid Effect: {SCP207Data.Effect} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP207Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP207Data.Duration} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP207Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP207Data.Intensity} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }

                    LogManager.Debug($"Applying effect {SCP207Data.Effect} at intensity {SCP207Data.Intensity}, duration is {SCP207Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP207Data.Effect;
                    float Duration = SCP207Data.Duration;
                    byte Intensity = SCP207Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP1853)
                {
                    if (SCP1853Data.Effect == null)
                    {
                        LogManager.Warn($"Invalid Effect: {SCP1853Data.Effect} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1853Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP1853Data.Duration} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1853Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP1853Data.Intensity} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }

                    LogManager.Debug($"Applying effect {SCP1853Data.Effect} at intensity {SCP1853Data.Intensity}, duration is {SCP1853Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP1853Data.Effect;
                    float Duration = SCP1853Data.Duration;
                    byte Intensity = SCP1853Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP1576)
                {
                    if (SCP1576Data.Effect == null)
                    {
                        LogManager.Warn($"Invalid Effect: {SCP1576Data.Effect} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1576Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP1576Data.Duration} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1576Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP1576Data.Intensity} for ID: {customitem.CustomItem.Id} Name: {customitem.CustomItem.Name}");
                        return;
                    }

                    LogManager.Debug($"Applying effect {SCP1576Data.Effect} at intensity {SCP1576Data.Intensity}, duration is {SCP1576Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP1576Data.Effect;
                    float Duration = SCP1576Data.Duration;
                    byte Intensity = SCP1576Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem2))
            {
                ISCP207Data SCP207Data = CustomItem2.CustomItem.CustomData as ISCP207Data;
                if (ev.Item.Type == ItemType.SCP207 || ev.Item.Type == ItemType.AntiSCP207)
                {
                    if (SCP207Data.RemoveItemAfterUse == false)
                    {
                        new SummonedCustomItem(CustomItem2.CustomItem, ev.Player);
                    }
                }
                ISCP1853Data SCP1853Data = CustomItem2.CustomItem.CustomData as ISCP1853Data;
                if (ev.Item.Type == ItemType.SCP1853)
                {
                    if (SCP1853Data.RemoveItemAfterUse == false)
                    {
                        new SummonedCustomItem(CustomItem2.CustomItem, ev.Player);
                    }
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem SummonedCustomItem))
            {
                if (SummonedCustomItem.Item.Type == ItemType.Adrenaline || SummonedCustomItem.Item.Type == ItemType.Medkit || SummonedCustomItem.Item.Type == ItemType.Painkillers)
                {
                    SummonedCustomItem.HandleCustomAction(SummonedCustomItem.Item);
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem3) && CustomItem3.CustomItem.CustomFlags.HasValue && CustomItem3.CustomItem.CustomFlags.Value.HasFlag(CustomFlags.SwitchRoleOnUse)) // Testing a new system for CustomFlags. Hopefully this will fix some bugs
            {
                foreach (SwitchRoleOnUseSettings SwitchRoleOnUseSettings in CustomItem3.CustomItem.FlagSettings.SwitchRoleOnUseSettings)
                {
                    if (SwitchRoleOnUseSettings.RoleId == null || SwitchRoleOnUseSettings.RoleType == null)
                    {
                        LogManager.Warn($"{CustomItem3.CustomItem.Name} field role_id or role_type is null aborting...");
                        break;
                    }
                    
                    if (SwitchRoleOnUseSettings.RoleType == "ECR")
                    {
                        if (CustomRole.TryGet((uint)SwitchRoleOnUseSettings.RoleId, out CustomRole? ECRRole))
                        {
                            if (SwitchRoleOnUseSettings.Delay is not null || SwitchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                {
                                    ECRRole.AddRole(ev.Player);
                                });
                            }
                            else
                            {
                                ECRRole.AddRole(ev.Player);
                            }
                            if (SwitchRoleOnUseSettings.KeepLocation != null || SwitchRoleOnUseSettings.KeepLocation != false)
                            {
                                Vector3 OldPos = ev.Player.Position;
                                Timing.CallDelayed(0.1f, () =>
                                {
                                    ev.Player.Position = OldPos;
                                });
                            }
                            break;
                        }
                        else
                        {
                            LogManager.Warn($"{SwitchRoleOnUseSettings.RoleId} Is not a ECR role");
                        }
                    }
                    else if (SwitchRoleOnUseSettings.RoleType == "UCR")
                    {
                        if (UCR.TryGetCustomRole((int)SwitchRoleOnUseSettings.RoleId, out _))
                        {
                            if (SwitchRoleOnUseSettings.Delay is not null || SwitchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                {
                                    UCR.GiveCustomRole((int)SwitchRoleOnUseSettings.RoleId, ev.Player);
                                });
                            }
                            else
                            {
                                UCR.GiveCustomRole((int)SwitchRoleOnUseSettings.RoleId, ev.Player);
                            }
                            if (SwitchRoleOnUseSettings.KeepLocation != null || SwitchRoleOnUseSettings.KeepLocation != false)
                            {
                                Vector3 OldPos = ev.Player.Position;
                                Timing.CallDelayed(0.1f, () =>
                                {
                                    ev.Player.Position = OldPos;
                                });
                            }
                            break;
                        }
                        else
                        {
                            LogManager.Warn($"{SwitchRoleOnUseSettings.RoleId} Is not a UCR role");
                        }
                    }
                    else if (SwitchRoleOnUseSettings.RoleType == "Normal")
                    {
                        if (ev.Player.Role != (RoleTypeId)SwitchRoleOnUseSettings.RoleId)
                        {
                            if (SwitchRoleOnUseSettings.Delay is not null || SwitchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                {
                                    ev.Player.Role.Set((RoleTypeId)SwitchRoleOnUseSettings.RoleId, SpawnReason.ItemUsage, (RoleSpawnFlags)SwitchRoleOnUseSettings.SpawnFlags);
                                });
                            }
                            else
                            {
                                ev.Player.Role.Set((RoleTypeId)SwitchRoleOnUseSettings.RoleId, SpawnReason.ItemUsage, (RoleSpawnFlags)SwitchRoleOnUseSettings.SpawnFlags);
                            }
                            break;
                        }
                    }
                    else if (SwitchRoleOnUseSettings.RoleType != "ECR" || SwitchRoleOnUseSettings.RoleType != "UCR" || SwitchRoleOnUseSettings.RoleType != "Normal")
                    {
                        LogManager.Warn($"The role_type field in {CustomItem3.CustomItem.Name} is currently {SwitchRoleOnUseSettings.RoleType} and should be 'Normal', 'UCR', or 'ECR'");
                    }
                }
            }
        }

        public void GrenadeExploding(ExplodingGrenadeEventArgs ev)
        {   
            if (Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<SpawnItemWhenDetonated>())
            {
                LogManager.Debug($"{ev.Projectile.Type} is a CustomItem");
                foreach (SpawnItemWhenDetonatedSettings SpawnItemWhenDetonatedSettings in CustomItem.CustomItem.FlagSettings.SpawnItemWhenDetonatedSettings)
                {
                    int Chance = UnityEngine.Random.Range(0, 100);
                    if (Chance <= SpawnItemWhenDetonatedSettings.Chance)
                    {
                        LogManager.Debug($"Loaded FlagSettings.");
                        if (SpawnItemWhenDetonatedSettings.ItemToSpawn == ItemType.SCP244a || SpawnItemWhenDetonatedSettings.ItemToSpawn == ItemType.SCP244b)
                        {
                            LogManager.Debug($"ItemToSpawn is SCP244a or SCP244b");
                            Scp244Pickup Scp244Pickup = (Scp244Pickup)Pickup.CreateAndSpawn(SpawnItemWhenDetonatedSettings.ItemToSpawn, ev.Position, null, ev.Player);
                            Scp244Pickup.MaxDiameter = 0.1f;
                            Scp244Pickup.State = Scp244State.Active;

                            if (!SpawnItemWhenDetonatedSettings.Pickupable ?? false)
                            {
                                Scp244Pickup.Weight = 5000f;
                            }
                            if (SpawnItemWhenDetonatedSettings.TimeTillDespawn != null || SpawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                Timing.RunCoroutine(TimeTillDespawnCoroutine(Scp244Pickup.Serial, (float)SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                            }
                        }
                        else
                        {
                            Pickup Pickup = Pickup.CreateAndSpawn(SpawnItemWhenDetonatedSettings.ItemToSpawn, ev.Position, null, ev.Player);
                            Vector3 Vector3 = new(0f, 1f, 0f);
                            Pickup.Transform.position = Pickup.Transform.position + Vector3;

                            if (!SpawnItemWhenDetonatedSettings.Pickupable ?? false)
                            {
                                Pickup.Weight = 5000f;
                            }
                            if (SpawnItemWhenDetonatedSettings.TimeTillDespawn != null || SpawnItemWhenDetonatedSettings.TimeTillDespawn > 0f)
                            {
                                LogManager.Debug($"Starting Despawn Coroutine");
                                Timing.RunCoroutine(TimeTillDespawnCoroutine(ev.Projectile.Serial, (float)SpawnItemWhenDetonatedSettings.TimeTillDespawn));
                            }
                        }
                    }
                }
            }
            else
            {
                LogManager.Debug($"{ev.Projectile.Type} is not a CustomItem with the SpawnItemWhenDetonated flag. Serial: {ev.Projectile.Serial}");
            }
            if (Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem Customitem) && CustomItem.HasModule<Cluster>())
            {
                LogManager.Debug($"{ev.Projectile.Type} is a CustomItem");
                foreach (ClusterSettings ClusterSettings in CustomItem.CustomItem.FlagSettings.ClusterSettings)
                {
                    Vector3 Scale = CustomItem.CustomItem.Scale * 0.75f;
                    if (ClusterSettings.ItemToSpawn == ItemType.GrenadeHE)
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            ExplosiveGrenade Grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                            for (int i = 0; i <= ClusterSettings.AmountToSpawn; i++)
                            {
                                Grenade.Scale = Scale;
                                Grenade.FuseTime = ClusterSettings.FuseTime ?? 5f;
                                Grenade.ScpDamageMultiplier = ClusterSettings.ScpDamageMultiplier ?? 1f;
                                Grenade.ChangeItemOwner(null, ev.Player);
                                Grenade.SpawnActive(ClusterOffset(ev.Position), owner: ev.Player);
                            }
                        });
                    }
                    else if (ClusterSettings.ItemToSpawn == ItemType.GrenadeFlash)
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            FlashGrenade FlashGrenade = (FlashGrenade)Item.Create(ItemType.GrenadeFlash);
                            for (int i = 0; i <= ClusterSettings.AmountToSpawn; i++)
                            {
                                FlashGrenade.FuseTime = ClusterSettings.FuseTime ?? 5f;
                                FlashGrenade.Scale = Scale;
                                FlashGrenade.ChangeItemOwner(null, ev.Player);
                                FlashGrenade.SpawnActive(ClusterOffset(ev.Position), owner: ev.Player);
                            }
                        });
                    }
                    else if (ClusterSettings.ItemToSpawn == ItemType.SCP018)
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            Scp018 SCP018 = (Scp018)Item.Create(ItemType.GrenadeFlash);
                            for (int i = 0; i <= ClusterSettings.AmountToSpawn; i++)
                            {
                                SCP018.FuseTime = ClusterSettings.FuseTime ?? 5f;
                                SCP018.Scale = Scale;
                                SCP018.ChangeItemOwner(null, ev.Player);
                                SCP018.SpawnActive(ClusterOffset(ev.Position), owner: ev.Player);
                            }
                        });
                    }
                    else
                    {
                        Timing.CallDelayed(0.1f, () =>
                        {
                            for (int i = 0; i <= ClusterSettings.AmountToSpawn; i++)
                            {
                                Pickup Pickup = Pickup.Create(ClusterSettings.ItemToSpawn);
                                Pickup.Scale = Scale;
                                Pickup.PreviousOwner = ev.Player;
                                Pickup.Spawn(ClusterOffset(ev.Position), previousOwner: ev.Player);
                            }
                        });
                    }
                }
            }
            else
            {
                LogManager.Debug($"{ev.Projectile.Type} is not a CustomItem with the Cluster flag. Serial: {ev.Projectile.Serial}");
            }
            DetonationPosition = ev.Position; // Untested
        }

        /// <summary>
        /// A coroutine that destroys a pickup by its serial after a set amount of time.
        /// </summary>
        public IEnumerator<float> TimeTillDespawnCoroutine(ushort Serial, float DespawnTime)
        {
            yield return Timing.WaitForSeconds(DespawnTime);
            Pickup Pickup = Pickup.Get(Serial);
            if (Pickup != null)
            {
                Pickup.Destroy();
                LogManager.Debug($"Destroyed pickup. Type: {Pickup.Type} Previous owner: {Pickup.PreviousOwner} Serial: {Pickup.Serial}");
            }
        }

        public void ThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<EffectWhenUsed>())
            {
                foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Item != null)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            if (EffectSettings.EffectEvent == "EffectWhenUsed")
                            {
                                if (EffectSettings.Effect == null)
                                {
                                    LogManager.Warn($"Invalid Effect: {EffectSettings.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectDuration < -1)
                                {
                                    LogManager.Warn($"Invalid Duration: {EffectSettings.EffectDuration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {EffectSettings.EffectIntensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }

                                LogManager.Debug($"Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Player}");
                                EffectType Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player.EnableEffect(Effect, Intensity, Duration, true);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectWhenUsed Flag was triggered but couldnt be ran for {CustomItem.CustomItem.Name}");
                    }
                }
            }
        }
        public void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
        {

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) && customItem.HasModule<WorkstationBan>())
            {
                if (ev.Player != null)
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
                }
                else
                {
                    LogManager.Error($"WorkstationBan flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                }
            }
            else return;
        }
        public void OnWorkstationActivation(ActivatingWorkstationEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null)
                return;

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) && customItem.HasModule<WorkstationBan>())
            {
                if (ev.Player != null)
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
                }
                else
                {
                    LogManager.Error($"WorkstationBan flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                }
            }
            else return;
        }

        public void OnDrop(DroppedItemEventArgs ev)
        {
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem customItem) && customItem.HasModule<ItemGlow>())
            {
                foreach (ItemGlowSettings ItemGlowSettings in customItem.CustomItem.FlagSettings.ItemGlowSettings)
                {
                    if (ev.Player != null)
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
                    else
                    {
                        LogManager.Error("ItemGlow flag was triggered but couldnt be ran.");
                    }
                }
            }
            else return;
        }
        public void OnShot(ShotEventArgs ev)
        {
            if (ev.Position == null)
                return;

            if (ev.Firearm == null)
                return;

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) && customItem.HasModule<EffectWhenUsed>())
            {
                foreach (EffectSettings EffectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Item != null)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            LogManager.Debug($"Checking if {EffectSettings.EffectEvent} = EffectWhenUsed");
                            if (EffectSettings.EffectEvent == "EffectWhenUsed")
                            {
                                LogManager.Debug($"{EffectSettings.EffectEvent} = EffectWhenUsed");
                                if (EffectSettings.Effect == null)
                                {
                                    LogManager.Warn($"Invalid Effect: {EffectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectDuration <= -2)
                                {
                                    LogManager.Warn($"Invalid Duration: {EffectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {EffectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                    return;
                                }
                                LogManager.Debug($"Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Player}");
                                EffectType Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectWhenUsed Flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                    }
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<ExplosiveBullets>())
            {
                foreach (ExplosiveBulletsSettings ExplosiveBulletsSettings in CustomItem.CustomItem.FlagSettings.ExplosiveBulletsSettings)
                {
                    if (ev.Firearm != null)
                    {
                        ev.CanSpawnImpactEffects = false;
                        Vector3 Position = ev.Position;
                        ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                        float DamageRadius = ExplosiveBulletsSettings.DamageRadius ?? 1f;
                        grenade.MaxRadius = DamageRadius;
                        grenade.FuseTime = .01f;
                        grenade.SpawnActive(Position, ev.Player);
                    }
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem Customitem) && Customitem.HasModule<ToolGun>())
            {
                ev.CanSpawnImpactEffects = false;
                ev.CanHurt = false;
                Vector3 RelativePosition = ev.Player.CurrentRoom.transform.InverseTransformPoint(ev.Position);
                LogManager.Info($"Triggered by {ev.Player.DisplayNickname}. Relative position inside {ev.Player.CurrentRoom.Name}: {RelativePosition}");
                ev.Player.ShowHint($"Relative position inside {ev.Player.CurrentRoom.Name}: {RelativePosition}. This was also sent to the console.");
                Vector3 Scale = new(0.2f, 0.2f, 0.2f);
                var primitive = Primitive.Create(ev.Position);
                primitive.Type = PrimitiveType.Cube;
                primitive.Color = new Vector4(255, 0, 0, -1);
                primitive.Scale = Scale;
                primitive.Collidable = false;
                primitive.GameObject.name = RelativePosition.ToString();
            }
            else return;
        }
        public void OnShot2(ShotEventArgs ev)
        {
            if (ev.Position == null)
                return;

            if (ev.Target == null)
                return;

            if (ev.Firearm == null)
                return;

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<EffectShot>())
            {
                foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Item != null)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            if (EffectSettings.EffectEvent == "EffectShot")
                            {
                                if (EffectSettings.Effect == null)
                                {
                                    LogManager.Warn($"Invalid Effect: {EffectSettings.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectDuration <= -2)
                                {
                                    LogManager.Warn($"Invalid Duration: {EffectSettings.EffectDuration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {EffectSettings.EffectIntensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                    return;
                                }

                                LogManager.Debug($"Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Target.DisplayNickname}");
                                EffectType Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Target?.EnableEffect(Effect, Intensity, Duration, true);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectShot Flag was triggered but couldnt be ran for {CustomItem.CustomItem.Name}.");
                    }
                }
            }
            else return;
        }

        public void OnCharge(ChargingJailbirdEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null)
                return;

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) && CustomItem.HasModule<NoCharge>())
            {
                if (ev.Item != null)
                {
                    ev.IsAllowed = false;
                    Timing.CallDelayed(0.1f, () =>
                    {
                        ev.Player.CurrentItem = ev.Item;
                    });
                }
            }
            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) && customItem.HasModule<EffectWhenUsed>())
            {
                AudioApi AudioApi = new();
                if (ev.Item != null)
                {
                    LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                    AudioApi.PlayAudio(customItem, ev.Player.Position);
                }
            }
            else return;
        }
        
        public void Receivingeffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Effect == null)
                return;
            if (ev.Player == null || ev.Player.CurrentItem == null)
                return;

            if (ev.Player != null && Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                LogManager.Debug($"{ev.Player.DisplayNickname} is reciving {ev.Effect}.");
                ISCP207Data SCP207Data = CustomItem.CustomItem.CustomData as ISCP207Data;
                if (ev.Effect.GetType() == typeof(Scp207) || ev.Effect.GetType() == typeof(AntiScp207))
                {
                    LogManager.Debug("Effect is from a 207 custom item.");
                    if (SCP207Data.Apply207Effect == false)
                    {
                        LogManager.Debug("Removing SCP-207 effect.");
                        ev.Intensity = 0;
                    }
                }
                ISCP1853Data SCP1853Data = CustomItem.CustomItem.CustomData as ISCP1853Data;
                if (ev.Effect.GetType() == typeof(Scp1853))
                {
                    LogManager.Debug("Effect is from a 1853 custom item.");
                    if (SCP1853Data.Apply1853Effect == false)
                    {
                        LogManager.Debug("Removing SCP-1853 effect.");
                        ev.Intensity = 0;
                    }
                }
            }
        }
        public void OnPickup(PickupDestroyedEventArgs ev)
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

        // Debugging Events.
        /// <summary>
        /// The debugging event for dropping a <see cref="Item"/>
        /// </summary>
        public void Ondrop(DroppingItemEventArgs ev)
        {
            if (ev.Item == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is dropping {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for adding a <see cref="Item"/>
        /// </summary>
        public void Onpickup(ItemAddedEventArgs ev)
        {
            if (ev.Item == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is adding {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for using a <see cref="Item"/>
        /// </summary>
        public void Onuse(UsingItemEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is using {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for reloading a <see cref="Firearm"/>
        /// </summary>
        public void Onreloading(ReloadingWeaponEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is reloading {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for shooting a <see cref="Firearm"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onshooting(ShootingEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is shooting {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for throwing a <see cref="Throwable"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onthrown(ThrownProjectileEventArgs ev)
        {
            if (ev.Item == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} has thrown {CustomItem.CustomItem.Name}");
            }
            else return;
        }


        /// <summary>
        /// Destroys the <see cref="Light"/> on a <see cref="CustomItem"/> <see cref="Pickup"/>.
        /// <param name="Pickup"></param>
        /// </summary>
        public void DestroyLightOnPickup(Pickup Pickup)
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
                ActiveLights.Remove(Pickup);
                LogManager.Debug("Light successfully destroyed.");
            }
            else
            {
                return;
            }

        }
        private Vector3 ClusterOffset(Vector3 position)
        {
            System.Random random = new System.Random();
            float x = position.x - 1 + ((float)random.NextDouble() * random.Next(0, 3));
            float y = position.y;
            float z = position.z - 1 + ((float)random.NextDouble() * random.Next(0, 3));
            return new Vector3(x, y, z);
        }
    }
}