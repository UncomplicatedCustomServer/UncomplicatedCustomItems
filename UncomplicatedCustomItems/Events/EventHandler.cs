using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using Light = LabApi.Features.Wrappers.LightSourceToy;
using Mirror;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using CustomPlayerEffects;
using InventorySystem.Items.Usables.Scp244;
using MEC;
using PlayerRoles;
using UncomplicatedCustomItems.Enums;
using System.Linq;
using System;
using UserSettings.ServerSpecific;
using LabApi.Events.Arguments.PlayerEvents;
using UncomplicatedCustomItems.Events.Methods;
using UncomplicatedCustomItems.Extensions;
using LABAPI = LabApi.Features.Wrappers;
using UncomplicatedCustomItems.Interfaces;
using System.Globalization;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Firearms;
using LabApi.Features.Wrappers;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.ShotEvents;
using PlayerStatsSystem;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.Scp914Events;
using Interactables.Interobjects.DoorUtils;
using Player = LabApi.Features.Wrappers.Player;
using AdminToys;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;
using LabApi.Features.Extensions;
using UncomplicatedCustomItems.API.Wrappers;

namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public Dictionary<Pickup, Light> ActiveLights = [];
        /// <summary>
        /// The <see cref="Vector3"/> coordinates of the latest detonation point for a <see cref="ExplosiveGrenadeProjectile"/>.
        /// Triggered by the <see cref="GrenadeExploding"/> method.
        /// </summary>
        public Vector3 DetonationPosition { get; set; } = Vector3.zero;

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles equiped keycards 
        /// Triggered by the <see cref="OnChangedItem"/> method.
        /// </summary>
        public static Dictionary<ushort, SummonedCustomItem> EquipedKeycards = [];
        private static Dictionary<Player, CoroutineHandle> _relativePosCoroutine = [];
        private static Dictionary<Player, CoroutineHandle> _HumeShieldRegenCoroutine = [];
        private static Dictionary<Player, long> _damageTimes = [];
        internal static Dictionary<PrimitiveObjectToy, int> ToolGunPrimitives = [];
        internal static Dictionary<int, LABAPI.CapybaraToy> Capybara = [];
        private static readonly CachedLayerMask ToolGunMask = new("Default", "Door", "Glass");
        public void OnHurt(PlayerHurtEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker.CurrentItem == null || ev.Player == null)
                return;

            //LogManager.Debug("OnHurt event is being triggered"); this was really annoying when debugging.
            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem summonedCustomItem) || !summonedCustomItem.CustomItem.CustomFlags.HasValue)
                return;

            if (summonedCustomItem.HasModule(CustomFlags.LifeSteal))
            {
                foreach (LifeStealSettings LifeStealSettings in summonedCustomItem.CustomItem.FlagSettings.LifeStealSettings)
                {
                    if (Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem CustomItem) && CustomItem.CustomItem.CustomFlags.HasValue && CustomItem.HasModule(CustomFlags.LifeSteal))
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
            if (summonedCustomItem.HasModule(CustomFlags.EffectShot))
            {
                foreach (EffectSettings EffectSettings in summonedCustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Player.CurrentItem != null)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            if (EffectSettings.EffectEvent == "EffectShot")
                            {
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
                                {
                                    LogManager.Warn($"Invalid Effect: {EffectSettings.Effect} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectDuration <= -2)
                                {
                                    LogManager.Warn($"Invalid Duration: {EffectSettings.EffectDuration} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                    return;
                                }
                                if (EffectSettings.EffectIntensity <= 0)
                                {
                                    LogManager.Warn($"Invalid intensity: {EffectSettings.EffectIntensity} for ID: {summonedCustomItem.CustomItem.Id} Name: {summonedCustomItem.CustomItem.Name}");
                                    return;
                                }

                                LogManager.Debug($"Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Player.Nickname}");
                                string Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player?.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {summonedCustomItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectShot Flag was triggered but couldnt be ran for {summonedCustomItem.CustomItem.Name}.");
                    }
                }
            }
        }
        public void OnTriggeringTesla(PlayerTriggeringTeslaEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || !ev.IsAllowed)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.DoNotTriggerTeslaGates))
                ev.IsAllowed = false;
        }

        public void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.FirearmItem == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem))
                return;

            if (ev.FirearmItem.Base.TryGetModule(out IAdsModule module) && module.AdsTarget)
            {
                if (ev.FirearmItem.Type != ItemType.GunSCP127)
                {
                    IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                    customItem.HitscanHitregModule.BaseBulletInaccuracy = data.AimingInaccuracy;
                }
                else
                {
                    ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                    customItem.Scp127Hitscan.BaseBulletInaccuracy = data.AimingInaccuracy;
                }
            }
            else if (ev.FirearmItem.Type != ItemType.GunSCP127)
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                customItem.HitscanHitregModule.BaseBulletInaccuracy = data.Inaccuracy;
            }
            else
            {
                ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                customItem.Scp127Hitscan.BaseBulletInaccuracy = data.Inaccuracy;
            }

            if (!customItem.CustomItem.CustomFlags.HasValue || !customItem.HasModule(CustomFlags.None))
                return;

            if (customItem.HasModule(CustomFlags.InfiniteAmmo))
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                customItem.MagazineModule.AmmoStored = data.MaxMagazineAmmo;
                customItem.MagazineModule.ServerResyncData();
                LogManager.Silent($"InfiniteAmmo flag was triggered: magazine refilled to {data.MaxMagazineAmmo}");
            }
            if (customItem.HasModule(CustomFlags.CustomSound))
            {
                AudioApi AudioApi = new();
                LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                AudioApi.PlayAudio(customItem, ev.Player.Position);
            }
            if (customItem.HasModule(CustomFlags.DieOnUse))
            {
                foreach (DieOnUseSettings DieOnUseSettings in customItem.CustomItem.FlagSettings.DieOnUseSettings)
                {
                    if (DieOnUseSettings.Vaporize ?? false)
                    {
                        Firearm firearm = new();
                        firearm.ItemTypeId = ItemType.ParticleDisruptor;
                        ev.Player.Damage(new DisruptorDamageHandler(new DisruptorShotEvent(FirearmItem.Get(firearm).Base, DisruptorActionModule.FiringState.FiringSingle), Vector3.up, -1));
                    }
                    if (DieOnUseSettings.DeathMessage != null)
                        ev.Player.Kill($"{DieOnUseSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                    else
                        ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                }
                LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} killed.");
            }
        }

        public void OnItemUse(PlayerUsedItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.UsableItem == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.DieOnUse))
            {
                foreach (DieOnUseSettings DieOnUseSettings in customItem.CustomItem.FlagSettings.DieOnUseSettings)
                {
                    if (DieOnUseSettings.Vaporize ?? false)
                    {
                        Firearm firearm = new();
                        firearm.ItemTypeId = ItemType.ParticleDisruptor;
                        ev.Player.Damage(new DisruptorDamageHandler(new DisruptorShotEvent(FirearmItem.Get(firearm).Base, DisruptorActionModule.FiringState.FiringSingle), Vector3.up, -1));
                    }
                    if (DieOnUseSettings.DeathMessage != null)
                        ev.Player.Kill($"{DieOnUseSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                    else
                        ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                }
            }
            if (customItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings EffectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    
                    if (EffectSettings.EffectEvent != null)
                    {
                        if (EffectSettings.EffectEvent == "EffectWhenUsed")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
                            {
                                LogManager.Warn($"Invalid Effect: {EffectSettings.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (EffectSettings.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {EffectSettings.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (EffectSettings.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {EffectSettings.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Player}");
                            string Effect = EffectSettings.Effect;
                            float Duration = EffectSettings.EffectDuration;
                            byte Intensity = EffectSettings.EffectIntensity;
                            ev.Player.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"{nameof(OnItemUse)}: No FlagSettings found on {customItem.CustomItem.Name}");
                    }
                }
            }
            if (customItem.HasModule(CustomFlags.CustomSound))
            {
                AudioApi AudioApi = new();
                LogManager.Debug($"{nameof(OnItemUse)}: Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {customItem.CustomItem.Name}.");
                AudioApi.PlayAudio(customItem, ev.Player.Position);
            }
            if (customItem.CustomItem.CustomFlags.Value.HasFlag(CustomFlags.SwitchRoleOnUse))
                SwitchRoleOnUseMethod.Start(customItem, ev.Player);
            // End of CustomFlags

            if (Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem CustomItem))
            {
                ISCP500Data SCP500Data = CustomItem.CustomItem.CustomData as ISCP500Data;
                ISCP207Data SCP207Data = CustomItem.CustomItem.CustomData as ISCP207Data;
                ISCP1853Data SCP1853Data = CustomItem.CustomItem.CustomData as ISCP1853Data;
                ISCP1576Data SCP1576Data = CustomItem.CustomItem.CustomData as ISCP1576Data;
                if (ev.UsableItem.Type == ItemType.SCP500)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == SCP500Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {SCP500Data.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP500Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP500Data.Duration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP500Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP500Data.Intensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP500Data.Effect} at intensity {SCP500Data.Intensity}, duration is {SCP500Data.Duration} to {ev.Player.Nickname}");
                    string Effect = SCP500Data.Effect;
                    float Duration = SCP500Data.Duration;
                    byte Intensity = SCP500Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP207 || ev.UsableItem.Type == ItemType.AntiSCP207)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == SCP207Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {SCP207Data.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP207Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP207Data.Duration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP207Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP207Data.Intensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP207Data.Effect} at intensity {SCP207Data.Intensity}, duration is {SCP207Data.Duration} to {ev.Player.Nickname}");
                    string Effect = SCP207Data.Effect;
                    float Duration = SCP207Data.Duration;
                    byte Intensity = SCP207Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP1853)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == SCP1853Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {SCP1853Data.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1853Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP1853Data.Duration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1853Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP1853Data.Intensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP1853Data.Effect} at intensity {SCP1853Data.Intensity}, duration is {SCP1853Data.Duration} to {ev.Player.Nickname}");
                    string Effect = SCP1853Data.Effect;
                    float Duration = SCP1853Data.Duration;
                    byte Intensity = SCP1853Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP1576)
                {
                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == SCP1576Data.Effect))
                    {
                        LogManager.Warn($"Invalid Effect: {SCP1576Data.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1576Data.Duration <= -2)
                    {
                        LogManager.Warn($"Invalid Duration: {SCP1576Data.Duration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    if (SCP1576Data.Intensity <= 0)
                    {
                        LogManager.Warn($"Invalid intensity: {SCP1576Data.Intensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                        return;
                    }
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP1576Data.Effect} at intensity {SCP1576Data.Intensity}, duration is {SCP1576Data.Duration} to {ev.Player.Nickname}");
                    string Effect = SCP1576Data.Effect;
                    float Duration = SCP1576Data.Duration;
                    byte Intensity = SCP1576Data.Intensity;
                    ev.Player?.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, true);
                }
                if (ev.UsableItem.Type == ItemType.SCP207 || ev.UsableItem.Type == ItemType.AntiSCP207)
                    if (SCP207Data.RemoveItemAfterUse == false)
                        new SummonedCustomItem(CustomItem.CustomItem, ev.Player);
                if (ev.UsableItem.Type == ItemType.SCP1853)
                    if (SCP1853Data.RemoveItemAfterUse == false)
                        new SummonedCustomItem(CustomItem.CustomItem, ev.Player);
                if (CustomItem.Item.Type == ItemType.Adrenaline || CustomItem.Item.Type == ItemType.Medkit || CustomItem.Item.Type == ItemType.Painkillers)
                    CustomItem.HandleCustomAction(CustomItem.Item);
            }
        }

        public void OnChangedItem(PlayerChangedItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost)
                return;
            if (ev.NewItem is not null)
            {
                if (!Utilities.TryGetSummonedCustomItem(ev.NewItem.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                    return;

                if (CustomItem.CustomItem.Item == ItemType.GunSCP127 && CustomItem.CustomItem.CustomItemType == CustomItemType.SCPItem)
                {
                    ISCP127Data data = CustomItem.CustomItem.CustomData as ISCP127Data;
                    Scp127Tier tier = Scp127TierManagerModule.GetTierForItem(CustomItem.Item.Base);
                    StartHumeShieldRegen(ev.Player, data, tier, CustomItem);
                }

                if (CustomItem.HasModule(CustomFlags.EffectWhenEquiped))
                {
                    foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            if (EffectSettings.EffectEvent == "EffectWhenEquiped")
                            {
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
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

                                LogManager.Debug($"{nameof(OnChangedItem)}: Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Player}");
                                string Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"{nameof(OnChangedItem)}: No FlagSettings found on {CustomItem.CustomItem.Name}");
                        }
                    }
                }
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                    EquipedKeycards.TryAdd(CustomItem.Serial, CustomItem);
                if (CustomItem.HasModule(CustomFlags.ToolGun))
                {
                    SSS.AddToolGunSettingsToUser(ev.Player.ReferenceHub);
                    StartRelativePosCoroutine(ev.Player);
                }
            }
            if (ev.OldItem != null)
            {
                if (Utilities.TryGetSummonedCustomItem(ev.OldItem.Serial, out SummonedCustomItem customItem))
                    if (customItem.HasModule(CustomFlags.ToolGun))
                    {
                        SSTwoButtonsSetting clearList = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 23);
                        foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                            if (ToolGunPrimitives.TryGetValue(primitive, out int iD))
                                if (clearList.SyncIsA)
                                    if (ev.Player.PlayerId == iD)
                                        primitive.Destroy();
                    }
            }
        }

        internal static void StartHumeShieldRegen(Player player, ISCP127Data data, Scp127Tier tier, SummonedCustomItem customItem)
        {
            StopHumeShieldRegen(player);
            CoroutineHandle handle = Timing.RunCoroutine(HumeShieldRegeneration(player, data, tier, customItem));
            _HumeShieldRegenCoroutine[player] = handle;
        }

        internal static void StopHumeShieldRegen(Player player)
        {
            if (_relativePosCoroutine.TryGetValue(player, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                _HumeShieldRegenCoroutine.Remove(player);
            }
        }


        internal static IEnumerator<float> HumeShieldRegeneration(Player player, ISCP127Data data, Scp127Tier tier, SummonedCustomItem customItem)
        {
            float regenRate = 0f;
            float damagePause = 0f;

            switch (tier)
            {
                case Scp127Tier.Tier1:
                    regenRate = data.Tier1ShieldRegenRate;
                    damagePause = data.Tier1ShieldOnDamagePause;
                    break;
                case Scp127Tier.Tier2:
                    regenRate = data.Tier2ShieldRegenRate;
                    damagePause = data.Tier2ShieldOnDamagePause;
                    break;
                case Scp127Tier.Tier3:
                    regenRate = data.Tier3ShieldRegenRate;
                    damagePause = data.Tier3ShieldOnDamagePause;
                    break;
            }

            for (;;)
            {
                if (_damageTimes.TryGetValue(player, out long time))
                {
                    long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
                    player.HumeShieldRegenRate = (elapsed >= damagePause) ? regenRate : 0f;
                }
                else
                {
                    player.HumeShieldRegenRate = regenRate;
                }

                if (player.CurrentItem == null || player.CurrentItem.Serial != customItem.Serial)
                    yield break;

                yield return Timing.WaitForOneFrame;
            }
        }

        public void Onpickup(PlayerPickedUpItemEventArgs ev)
        {
            if (ev.Item == null)
                return;
            if (ev.Item.Category != ItemCategory.Armor)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;

            if (CustomItem.HasModule(CustomFlags.EffectWhenEquiped))
            {
                foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (EffectSettings.EffectEvent != null)
                    {
                        if (EffectSettings.EffectEvent == "EffectWhenEquiped")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
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
                            string Effect = EffectSettings.Effect;
                            float Duration = EffectSettings.EffectDuration;
                            byte Intensity = EffectSettings.EffectIntensity;
                            ev.Player.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                    }
                }
            }
            if (CustomItem.HasModule(CustomFlags.Capybara))
            {
                LABAPI.CapybaraToy capybara = LABAPI.CapybaraToy.Create(ev.Player.GameObject.transform);
                capybara.CollidersEnabled = false;
                capybara.Position = capybara.Position + new Vector3(0, -0.8f, 0);
                ev.Player.GameObject.transform.localScale = new(0.2f, 0.3f, 0.5f);
                capybara.Scale = new(6f, 4f, 2.8f);
                Capybara.TryAdd(ev.Player.PlayerId, capybara);
            }
        }

        public void OnRoundEnd(RoundEndingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            _damageTimes.Clear();
            Capybara.Clear();
        }

        public void GrenadeExploding(ProjectileExplodingEventArgs ev)
        {
            if (ev.TimedGrenade == null || ev.Player == null || ev.Position == null)
                return;
            DetonationPosition = ev.Position;
            if (!Utilities.TryGetSummonedCustomItem(ev.TimedGrenade.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;
            LogManager.Debug($"{ev.TimedGrenade.Type} is a CustomItem");
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
                        else if (SpawnItemWhenDetonatedSettings.ItemType == "Normal" || SpawnItemWhenDetonatedSettings.ItemType == "normal")
                        {
                            if ((ItemType)SpawnItemWhenDetonatedSettings.ItemId == ItemType.SCP244a || (ItemType)SpawnItemWhenDetonatedSettings.ItemId == ItemType.SCP244b)
                            {
                                LogManager.Debug($"Item is SCP244a or SCP244b");
                                Scp244Pickup scp244Pickup = (Scp244Pickup)Scp244Pickup.Create((ItemType)SpawnItemWhenDetonatedSettings.ItemId, ev.Position);
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
                                Pickup pickup = Pickup.Create((ItemType)SpawnItemWhenDetonatedSettings.ItemId, ev.Position);
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
                LogManager.Debug($"{ev.TimedGrenade.Type} is not a CustomItem with the SpawnItemWhenDetonated flag. Serial: {ev.TimedGrenade.Serial}");
            }
            if (CustomItem.HasModule(CustomFlags.Cluster))
            {
                LogManager.Debug($"{ev.TimedGrenade.Type} is a CustomItem");
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
                                LABAPI.ExplosiveGrenadeProjectile grenade = (LABAPI.ExplosiveGrenadeProjectile)LABAPI.ExplosiveGrenadeProjectile.SpawnActive(position, ClusterSettings.ItemToSpawn, ev.Player, (double)ClusterSettings.FuseTime);
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
                                LABAPI.Pickup pickup = LABAPI.Pickup.Create(ClusterSettings.ItemToSpawn, position, ev.Player.Rotation, Scale);
                                pickup.Spawn();
                            }
                        });
                    }
                }
            }
            else
            {
                LogManager.Debug($"{ev.TimedGrenade.Type} is not a CustomItem with the Cluster flag. Serial: {ev.TimedGrenade.Serial}");
            }
        }

        public void OnPickupUpgrade(Scp914ProcessingPickupEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out _))
            {
                ev.Pickup.Position = ev.NewPosition;
                ev.IsAllowed = false;
            }

            LogManager.Debug($"{nameof(OnPickupUpgrade)}: Triggered");
            foreach (CustomItem customItem in CustomItem.List)
            {
                LogManager.Debug($"{nameof(OnPickupUpgrade)}: {customItem.Name}");
                if (customItem.HasModule(CustomFlags.Craftable))
                {
                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: {customItem.Name} has Craftable CustomFlag");
                    foreach (CraftableSettings craftableSettings in customItem.FlagSettings.CraftableSettings)
                    {
                        LogManager.Debug($"{nameof(OnPickupUpgrade)}: Checking settings on {customItem.Name}");
                        if (craftableSettings.OriginalItem == null || craftableSettings.KnobSetting == null || craftableSettings.Chance == null)
                        {
                            LogManager.Warn($"{nameof(OnPickupUpgrade)}: {customItem.Name} - {customItem.Id} has OriginalItem, KnobSetting, or chance equal null. Aborting... \n Values: {craftableSettings.OriginalItem} {craftableSettings.KnobSetting} {craftableSettings.Chance}");
                            break;
                        }
                        else if (UnityEngine.Random.Range(0, 100) <= craftableSettings.Chance)
                        {
                            LogManager.Debug($"{nameof(OnPickupUpgrade)}: {customItem.Name} Passed chance");
                            try
                            {
                                LogManager.Debug($"{nameof(OnPickupUpgrade)}: Checking if {craftableSettings.OriginalItem} equals {ev.Pickup} and {craftableSettings.KnobSetting} equals {ev.KnobSetting}");
                                if (ev.Pickup.Type == craftableSettings.OriginalItem && ev.KnobSetting == craftableSettings.KnobSetting)
                                {
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: Check passed!");
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: Spawning {customItem.Name} at {ev.Pickup.Position}...");
                                    try
                                    {
                                        ev.Pickup.Destroy();
                                        new SummonedCustomItem(customItem, ev.NewPosition);
                                        LogManager.Debug($"{nameof(OnPickupUpgrade)}: CustomItem created successfully at {ev.NewPosition}");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnPickupUpgrade)}: Error during CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                                    }
                                }
                                else
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: {ev.KnobSetting} != {craftableSettings.KnobSetting} or {ev.Pickup} != {craftableSettings.OriginalItem}");
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"{nameof(OnPickupUpgrade)}: Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                }    
            }
        }
        
        public void OnItemUpgrade(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out _))
                ev.IsAllowed = false;

            LogManager.Debug($"{nameof(OnItemUpgrade)}: Triggered");
            foreach (CustomItem customItem in CustomItem.List)
            {
                LogManager.Debug($"{nameof(OnItemUpgrade)}: {customItem.Name}");
                if (customItem.HasModule(CustomFlags.Craftable))
                {
                    LogManager.Debug($"{nameof(OnItemUpgrade)}: {customItem.Name} has Craftable CustomFlag");
                    foreach (CraftableSettings craftableSettings in customItem.FlagSettings.CraftableSettings)
                    {
                        LogManager.Debug($"{nameof(OnItemUpgrade)}: Checking settings on {customItem.Name}");
                        if (craftableSettings.OriginalItem == null || craftableSettings.KnobSetting == null || craftableSettings.Chance == null)
                        {
                            LogManager.Warn($"{nameof(OnItemUpgrade)}: {customItem.Name} - {customItem.Id} has OriginalItem, KnobSetting, or Chance equals null. Aborting... \n Values: {craftableSettings.OriginalItem} {craftableSettings.KnobSetting} {craftableSettings.Chance}");
                            break;
                        }
                        else if (UnityEngine.Random.Range(0, 100) <= craftableSettings.Chance)
                        {
                            try
                            {
                                LogManager.Debug($"{nameof(OnItemUpgrade)}: Checking if {craftableSettings.OriginalItem} equals {ev.Player.CurrentItem.Type} and {craftableSettings.KnobSetting} equals {ev.KnobSetting}");
                                if (ev.Player.CurrentItem.Type == craftableSettings.OriginalItem && ev.KnobSetting == craftableSettings.KnobSetting)
                                {
                                    LogManager.Debug($"{nameof(OnItemUpgrade)}: Check passed!");
                                    LogManager.Debug($"{nameof(OnItemUpgrade)}: Giving {customItem.Name} to {ev.Player.Nickname}...");
                                    try
                                    {
                                        ev.Player.RemoveItem(ev.Item);
                                        new SummonedCustomItem(customItem, ev.Player);
                                        LogManager.Debug($"{nameof(OnItemUpgrade)}: Gave {customItem.Name} to {ev.Player.Nickname}...");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnItemUpgrade)}: Error during CustomItem creation: {ex.Message}\n{ex.StackTrace}");
                                    }
                                }
                                else
                                    LogManager.Debug($"{nameof(OnItemUpgrade)}: {ev.KnobSetting} != {craftableSettings.KnobSetting} or {ev.Item} != {craftableSettings.OriginalItem}");
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"{nameof(OnItemUpgrade)}: Exception: {ex.Message}\n{ex.StackTrace}");
                            }
                        }
                    }
                }
            }
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
                LogManager.Debug($"Destroyed pickup. Type: {Pickup.Type} Previous owner: {Pickup.LastOwner} Serial: {Pickup.Serial}");
            }
        }

        internal static void StartRelativePosCoroutine(Player player)
        {
            StopRelativePosCoroutine(player);
            CoroutineHandle handle = Timing.RunCoroutine(RelativePos(player));
            _relativePosCoroutine[player] = handle;
        }
        internal static void PauseRelativePosCoroutine(Player player)
        {
            if (_relativePosCoroutine.TryGetValue(player, out CoroutineHandle handle))
            {
                Timing.PauseCoroutines(handle);
                Timing.CallDelayed(6f, () =>
                {
                    Timing.ResumeCoroutines(handle);
                });
            }
        }
        internal static void StopRelativePosCoroutine(Player player)
        {
            if (_relativePosCoroutine.TryGetValue(player, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                _relativePosCoroutine.Remove(player);
            }
        }
        internal static IEnumerator<float> RelativePos(Player player)
        {
            try
            {
                for (; ;)
                {
                    if (player.CurrentItem == null)
                        StopRelativePosCoroutine(player);
                    else if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
                        if (!CustomItem.HasModule(CustomFlags.ToolGun))
                            StopRelativePosCoroutine(player);
                    if (player.Room == null)
                        PauseRelativePosCoroutine(player);

                    SSPlaintextSetting colorSetting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(player.ReferenceHub, 21);
                    SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(player.ReferenceHub, 22);

                    string deletioncolor = string.Empty;
                    bool deletionbool = false;
                    string DeletionMode = string.Empty;
                    string room = string.Empty;
                    if (deletionMode.SyncIsA)
                    {
                        DeletionMode = "ADS";
                        if (player.IsAimingDownWeapon())
                            deletionbool = true;
                        else
                            deletionbool = false;
                    }
                    else if (deletionMode.SyncIsB)
                    {
                        DeletionMode = "Flashlight Toggle";
                        if (player.FlashLightModuleEnabled())
                            deletionbool = true;
                        else
                            deletionbool = false;
                    }

                    if (deletionbool)
                        deletioncolor = "#00ff00";
                    else
                        deletioncolor = "#Ff0000";
                    if (player.Room.Name.ToString() != "Unnamed")
                        room = player.Room.Name.ToString();
                    else
                        room = player.Room.GameObject.name;
                    StringExtensions.TryParseVector3(colorSetting.SyncInputText, out Vector3 color);
                    string hexcolor = Vector3Extensions.ToHexColor(color);
                    string hinttext = $"<pos=-10em><voffset=-12.3em><color=Red>{player.Nickname} - {player.Role.GetFullName()}</color></voffset>\n<pos=-10em>{room} - <color=yellow>{player.Room.LocalPosition(player.Position)}</color>\n<pos=-10em>Primitive Color: <color={hexcolor}>{color}</color>\n<pos=-10em>Deletion Mode: {DeletionMode}\n<pos=-10em>Deleting: <color={deletioncolor}>{deletionbool}</color>";
                    player.SendHint($"<align=left>{hinttext}</align>", 0.5f);
                    yield return Timing.WaitForOneFrame;
                }
            }
            finally
            {
                if (_relativePosCoroutine.ContainsKey(player))
                        _relativePosCoroutine.Remove(player);
            }
        }

        public void OnValueReceived(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(referenceHub.gameObject, out Player player))
                return;

            SSTextArea textArea = ServerSpecificSettingsSync.GetSettingOfUser<SSTextArea>(player.ReferenceHub, 29);
            SSPlaintextSetting commandarg = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(player.ReferenceHub, 26);

            if (settingBase is SSButton devRoleButton && devRoleButton.SettingId == 28 && player.UserId == "76561199150506472@steam")
            {
                player.GroupName = "💻 UCI Lead Developer";
                player.GroupColor = "emerald";
                textArea.SendTextUpdate($"UCI Lead Developer group given to {player.Nickname}", true);
            }
            else if (settingBase is SSButton managerRoleButton && managerRoleButton.SettingId == 30 && player.UserId == "76561199150506472@steam")
            {
                player.GroupName = "🎲 UCS Studios Manager";
                player.GroupColor = "aqua";
                textArea.SendTextUpdate($"Manager group given to {player.Nickname}", true);
            }
            else if (settingBase is SSButton buttonSetting && buttonSetting.SettingId == 24 && player.UserId == "76561199150506472@steam")
            {
                Utilities.TryGetCustomItemByName("ToolGun", out ICustomItem customitem);
                new SummonedCustomItem(customitem, player);
                textArea.SendTextUpdate($"Successfuly gave ToolGun to {player.Nickname}", true);
            }
            else if (player.UserId != "76561199150506472@steam")
                LogManager.Warn($"{player.Nickname} Attempted to spawn a ToolGun with debugging SSS!");
            if (settingBase is SSKeybindSetting keybindSetting && keybindSetting.SettingId == 20 && keybindSetting.SyncIsPressed)
            {
                if (player.CurrentItem is null)
                {
                    foreach (Item item in player.Items)
                    {
                        if (item.Type.IsArmor())
                        {
                            if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem))
                            {
                                if (!player.Connection.isAuthenticated || player.Inventory == null)
                                    return;

                                customItem.HandleEvent(player, ItemEvents.SSSS, item.Serial);
                                break;
                            }
                            else
                                LogManager.Debug($"{nameof(OnValueReceived)}: {item} - {item.Serial} Is not a CustomItem.");
                        }
                    }
                }
                else if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem Item))
                    Item?.HandleEvent(player, ItemEvents.SSSS, player.CurrentItem.Serial);
            }
        }

        public void OnHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;

            _damageTimes.TryAdd(ev.Player, DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }

        public void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.Role == RoleTypeId.Spectator || ev.Player.Role == RoleTypeId.Destroyed)
                return;
            
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (Item item in ev.Player.Items)
                {
                    if (!Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                        return;

                    if (CustomItem.HasModule(CustomFlags.EffectWhenEquiped))
                    {
                        foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                        {
                            if (EffectSettings.EffectEvent != null)
                            {
                                if (EffectSettings.EffectEvent == "EffectWhenEquiped")
                                {
                                    if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
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
                                    string Effect = EffectSettings.Effect;
                                    float Duration = EffectSettings.EffectDuration;
                                    byte Intensity = EffectSettings.EffectIntensity;
                                    ev.Player.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                                }
                            }
                            else
                            {
                                LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                            }
                        }
                    }
                    if (CustomItem.HasModule(CustomFlags.Capybara))
                    {
                        LABAPI.CapybaraToy capybara = LABAPI.CapybaraToy.Create(ev.Player.GameObject.transform);
                        capybara.CollidersEnabled = false;
                        capybara.Position = capybara.Position + new Vector3(0, -0.8f, 0);
                        ev.Player.GameObject.transform.localScale = new(0.2f, 0.3f, 0.5f);
                        capybara.Scale = new(6f, 4f, 2.8f);
                        Capybara.TryAdd(ev.Player.PlayerId, capybara);
                    }
                }
            });
        }

        public void OnDoorInteracting(PlayerInteractedDoorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.CanOpen == false)
                return;
            if (ev.Door.Permissions == DoorPermissionFlags.None)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            // This probably will throw a error with plugins like RemoteKeycard
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData Data = CustomItem.CustomItem.CustomData as IKeycardData;
                    if (ev.Door.Base.IsMoving && Data.OneTimeUse)
                    {
                        Timing.CallDelayed(0.5f, () =>
                        {
                            ev.Player.SendHint($"{Data.OneTimeUseHint.Replace("%name%", CustomItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {CustomItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(CustomItem.Item);
                            CustomItem.ResetBadge(ev.Player);
                        });
                    }
                }
            }
        }
        public void OnGeneratorUnlock(PlayerUnlockingGeneratorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            if (ev.IsAllowed == false)
                return;
            // This probably will throw a error with plugins like RemoteKeycard
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData Data = CustomItem.CustomItem.CustomData as IKeycardData;
                    if (Data.OneTimeUse)
                    {
                        Timing.CallDelayed(0.5f, () =>
                        {
                            ev.Player.SendHint($"{Data.OneTimeUseHint.Replace("%name%", CustomItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {CustomItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(CustomItem.Item);
                        });
                    }
                }
            }
        }
        public void OnLockerInteracting(PlayerInteractingLockerEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            if (ev.IsAllowed == false)
                return;
            // This probably will throw a error with plugins like RemoteKeycard
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData Data = CustomItem.CustomItem.CustomData as IKeycardData;
                    if (ev.Chamber.IsOpen && Data.OneTimeUse)
                    {
                        Timing.CallDelayed(0.5f, () =>
                        {
                            ev.Player.SendHint($"{Data.OneTimeUseHint.Replace("%name%", CustomItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {CustomItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(CustomItem.Item);
                        });
                    }
                }
            }
        }
        public void WeaponFlashLight(PlayerToggledWeaponFlashlightEventArgs ev)
        {
            if (ev.FirearmItem == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem CustomItem))
                return;

            CustomItem.FlashLightToggle = ev.NewState;
        }
        public void FlippedCoin(PlayerFlippedCoinEventArgs ev)
        {
            if (ev.CoinItem == null || ev.Player == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.CoinItem.Serial, out SummonedCustomItem CustomItem))
                CustomItem.HandleEvent(ev.Player, ItemEvents.Use, ev.CoinItem.Serial);
            SwitchRoleOnUseMethod.Start(CustomItem, ev.Player);
        }
        public void ToggledFlashlight(PlayerToggledFlashlightEventArgs ev)
        {
            if (ev.LightItem == null || ev.Player == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.LightItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem != null)
                {
                    CustomItem.HandleEvent(ev.Player, ItemEvents.Use, ev.LightItem.Serial);
                    SwitchRoleOnUseMethod.Start(CustomItem, ev.Player);
                }
            }
        }

        public void ThrownProjectile(PlayerThrewProjectileEventArgs ev)
        {
            if (ev.Projectile == null || ev.Player == null || ev.Projectile == null)
                return;
            if (Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;
                
            if (CustomItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (EffectSettings.EffectEvent != null)
                    {
                        if (EffectSettings.EffectEvent == "EffectWhenUsed")
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
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
                            string Effect = EffectSettings.Effect;
                            float Duration = EffectSettings.EffectDuration;
                            byte Intensity = EffectSettings.EffectIntensity;
                            ev.Player.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                    }
                }
            }
        }

        public void OnDrop(PlayerDroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem SummonedCustomItem))
                return;

            if (SummonedCustomItem.HasModule(CustomFlags.Capybara))
            {
                if (Capybara.TryGetValue(ev.Player.PlayerId, out LABAPI.CapybaraToy capybara))
                {
                    capybara.Destroy();
                    ev.Player.GameObject.transform.localScale = new(1, 1, 1);
                }
            }
            if (SummonedCustomItem.HasModule(CustomFlags.ToolGun))
            {
                ev.Pickup.Destroy();
                SummonedCustomItem.List.Remove(SummonedCustomItem);
            }

            if (SummonedCustomItem.CustomItem.CustomFlags.HasValue && SummonedCustomItem.HasModule(CustomFlags.DieOnDrop))
            {
                foreach (DieOnDropSettings DieOnDropSettings in SummonedCustomItem.CustomItem.FlagSettings.DieOnDropSettings)
                {
                    LogManager.Debug($"Checking Vaporize setting for {SummonedCustomItem.CustomItem.Name}");
                    if (DieOnDropSettings.Vaporize != null && (bool)DieOnDropSettings.Vaporize)
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Debug($"{ev.Player.Nickname} is being vaporized by {SummonedCustomItem.CustomItem.Name}");
                            Firearm firearm = new();
                            firearm.ItemTypeId = ItemType.ParticleDisruptor;
                            ev.Player.Damage(new DisruptorDamageHandler(new DisruptorShotEvent(FirearmItem.Get(firearm).Base, DisruptorActionModule.FiringState.FiringSingle), Vector3.up, -1));
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Vaporize {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                    else
                        LogManager.Debug($"Vaporize settings were null or false for {SummonedCustomItem.CustomItem.Name}");
                    if (DieOnDropSettings.DeathMessage.Count() >= 1 && DieOnDropSettings.DeathMessage != null)
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            ev.Player.Kill($"{DieOnDropSettings.DeathMessage.Replace("%name%", SummonedCustomItem.CustomItem.Name)}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Kill {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                    else
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            ev.Player.Kill($"Killed by {SummonedCustomItem.CustomItem.Name}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Kill {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                }
            }
            else return;
        }

        public void OnPickupCreation(PickupCreatedEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem SummonedCustomItem))
            {
                try
                {
                    ev.Pickup.GameObject.transform.localScale = SummonedCustomItem.CustomItem.Scale;
                    ev.Pickup.Weight = SummonedCustomItem.CustomItem.Weight;
                }
                catch (Exception ex)
                {
                    LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                    LogManager.Error($"Couldnt set CustomItem Pickup Scale or CustomItem Pickup Weight\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                }
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
        public void OnDropping(PlayerDroppingItemEventArgs ev)
        {
            if (ev.Item is null || ev.Player == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;
            if (CustomItem.HasModule(CustomFlags.CantDrop))
            {
                ev.IsAllowed = false;
                foreach (CantDropSettings CantDropSettings in CustomItem.CustomItem.FlagSettings.CantDropSettings)
                {
                    if (CantDropSettings.HintOrBroadcast != null && CantDropSettings.HintOrBroadcast == "hint" || CantDropSettings.HintOrBroadcast == "Hint")
                    {
                        if (CantDropSettings.Message != null && CantDropSettings.Duration != null && CantDropSettings.Duration >= 1)
                        {
                            try
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} - {CustomItem.CustomItem.CustomFlags}");
                                LogManager.Debug($"Sending CantDrop Hint to {ev.Player.Nickname}\nHint: {CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}");
                                ev.Player.SendHint($"{CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}", (ushort)CantDropSettings.Duration);
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} - {CustomItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Hint to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                            }
                        }
                    }
                    else if (CantDropSettings.HintOrBroadcast != null && CantDropSettings.HintOrBroadcast == "broadcast" || CantDropSettings.HintOrBroadcast == "Broadcast")
                    {
                        if (CantDropSettings.Message != null && CantDropSettings.Duration != null && CantDropSettings.Duration >= 1)
                        {
                            try
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} - {CustomItem.CustomItem.CustomFlags}");
                                LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.Nickname}\nBroadcast: {CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}");
                                ev.Player.SendBroadcast($"{CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}", (ushort)CantDropSettings.Duration, Broadcast.BroadcastFlags.Normal, true);
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} - {CustomItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Broadcast to {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                            }
                        }
                    }
                    else
                    {
                        LogManager.Warn($"CantDropSettings HintOrBroadcast for {CustomItem.CustomItem.Name} is {CantDropSettings.HintOrBroadcast} Expected values are 'hint' or 'broadcast'");
                    }
                }
            }
            else return;
        }

        public void OnDying(PlayerDyingEventArgs ev)
        {
            if (ev.Player.Inventory != null)
            {
                foreach (Item item in ev.Player.Items)
                {
                    if (Utilities.TryGetSummonedCustomItem(item.Serial, out var sItem))
                    {
                        if (sItem.HasModule(CustomFlags.Capybara))
                        {
                            if (Capybara.TryGetValue(ev.Player.PlayerId, out LABAPI.CapybaraToy capybara))
                            {
                                capybara.Destroy();
                                ev.Player.GameObject.transform.localScale = new(1, 1, 1);
                            }
                        }
                    }
                }
            }

            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (!ev.Attacker.Connection.isReady)
                return;
            if (!ev.Player.Connection.isReady)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;
            if (!ev.Attacker.CurrentItem.Type.IsWeapon())
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.VaporizeKills))
            {
                try
                {
                    LogManager.Silent("Name | Id | CustomFlag(s)");
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Debug($"Vaporizing {ev.Player.Nickname}");
                    Firearm firearm = new();
                    firearm.ItemTypeId = ItemType.ParticleDisruptor;
                    ev.Player.Damage(new DisruptorDamageHandler(new DisruptorShotEvent(FirearmItem.Get(firearm).Base, DisruptorActionModule.FiringState.FiringSingle), Vector3.up, -1));
                }
                catch (Exception ex)
                {
                    LogManager.Silent("Name | Id | CustomFlag(s)");
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Error($"Couldnt Vaporize {ev.Player.Nickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                }
            }
            if (customItem.HasModule(CustomFlags.HealOnKill))
            {
                foreach (HealOnKillSettings healOnKillSettings in customItem.CustomItem.FlagSettings.HealOnKillSettings)
                {
                    LogManager.Debug($"{nameof(OnDying)}: Healing {ev.Attacker.Nickname} by {healOnKillSettings.HealAmount}");
                    if (ev.Attacker.Health != ev.Attacker.MaxHealth)
                        ev.Attacker.Heal(healOnKillSettings.HealAmount ?? 5f);
                    else if (healOnKillSettings.ConvertToAhpIfFull ?? false)
                    {
                        LogManager.Debug($"{nameof(OnDying)}: {ev.Attacker.Nickname} health is full and ConvertToAhpIfFull is true adding {healOnKillSettings.HealAmount} to {ev.Attacker.Nickname} AHP");
                        ev.Attacker.ArtificialHealth = healOnKillSettings.HealAmount ?? 5f;
                    }
                }
            }
        }
        public void OnVerified(PlayerJoinedEventArgs ev)
        {
            if (ev.Player.UserId == "76561199150506472@steam")
            {
                // Baguetter credit tag
                if (Plugin.Instance.Config.EnableCreditTags)
                {
                    ev.Player.GroupName = "💻 UCI Lead Developer";
                    ev.Player.GroupColor = "emerald";
                }
                if (Plugin.Instance.IsPrerelease)
                    SSS.AddDebugSettingsToUser(ev.Player.ReferenceHub);
                else
                    SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
            }
            else
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
        }
        public void OnLeft(PlayerLeftEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.IsHost)
                return;

            if (Capybara.ContainsKey(ev.Player.PlayerId))
                Capybara.TryRemove(ev.Player.PlayerId);
        }

        public void OnShot(PlayerShotWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings EffectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.FirearmItem != null)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            LogManager.Debug($"Checking if {EffectSettings.EffectEvent} = EffectWhenUsed");
                            if (EffectSettings.EffectEvent == "EffectWhenUsed")
                            {
                                LogManager.Debug($"{EffectSettings.EffectEvent} = EffectWhenUsed");
                                if (!ev.Player.ReferenceHub.playerEffectsController.AllEffects.Any(e => e.name == EffectSettings.Effect))
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
                                string Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player?.ReferenceHub.playerEffectsController.ChangeState(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
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
            if (Physics.Raycast(ev.Player.Camera.position, ev.Player.Camera.forward, out RaycastHit hitInfo, customItem.HitscanHitregModule.DamageFalloffDistance + customItem.HitscanHitregModule.FullDamageDistance, ToolGunMask))
            {
                if (customItem.HasModule(CustomFlags.ExplosiveBullets))
                {
                    foreach (ExplosiveBulletsSettings ExplosiveBulletsSettings in customItem.CustomItem.FlagSettings.ExplosiveBulletsSettings)
                    {
                        ExplosiveGrenadeProjectile grenade = (ExplosiveGrenadeProjectile)TimedGrenadeProjectile.SpawnActive(hitInfo.point, ItemType.GrenadeHE, ev.Player, 0.2);
                        grenade.MaxRadius = ExplosiveBulletsSettings.DamageRadius ?? 10f;
                        grenade.FuseEnd();
                    }
                }
                if (customItem.HasModule(CustomFlags.ToolGun))
                {
                    SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 22);
                    if (deletionMode.SyncIsA && ev.FirearmItem.IsAiming())
                    {
                        foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                        {
                            if (primitive.GameObject.name.Contains("UCI"))
                            {
                                Vector3 halfSize = primitive.Scale / 2f;
                                Vector3 minBounds = primitive.Position - halfSize;
                                Vector3 maxBounds = primitive.Position + halfSize;

                                if (hitInfo.point.x >= minBounds.x && hitInfo.point.x <= maxBounds.x && hitInfo.point.y >= minBounds.y && hitInfo.point.y <= maxBounds.y && hitInfo.point.z >= minBounds.z && hitInfo.point.z <= maxBounds.z)
                                    primitive.Destroy();
                            }
                        }
                    }
                    else if (deletionMode.SyncIsB && customItem.FlashLightToggle)
                    {
                        foreach (PrimitiveObjectToy primitive in AdminToy.List.OfType<PrimitiveObjectToy>().ToList())
                        {
                            if (primitive.GameObject.name.Contains("UCI"))
                            {
                                Vector3 halfSize = primitive.Scale / 2f;
                                Vector3 minBounds = primitive.Position - halfSize;
                                Vector3 maxBounds = primitive.Position + halfSize;

                                if (hitInfo.point.x >= minBounds.x && hitInfo.point.x <= maxBounds.x && hitInfo.point.y >= minBounds.y && hitInfo.point.y <= maxBounds.y && hitInfo.point.z >= minBounds.z && hitInfo.point.z <= maxBounds.z)
                                    primitive.Destroy();
                            }
                        }
                    }
                    else
                    {
                        PauseRelativePosCoroutine(ev.Player);
                        SSPlaintextSetting setting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(ev.Player.ReferenceHub, 21);
                        string room = string.Empty;
                        string[] components = setting.SyncInputText.Split(',');
                        Vector4 color = new();
                        if (components.Length == 4)
                        {
                            float x = float.Parse(components[0].Trim(), CultureInfo.InvariantCulture);
                            float y = float.Parse(components[1].Trim(), CultureInfo.InvariantCulture);
                            float z = float.Parse(components[2].Trim(), CultureInfo.InvariantCulture);
                            float w = float.Parse(components[3].Trim(), CultureInfo.InvariantCulture);

                            color = new Vector4(x, y, z, w);
                        }
                        if (ev.Player.Room.Name.ToString() != "Unnamed")
                            room = ev.Player.Room.Name.ToString();
                        else
                            room = ev.Player.Room.GameObject.name;
                        Vector3 RelativePosition = ev.Player.Room.LocalPosition(hitInfo.point);
                        LogManager.Info($"Triggered by {ev.Player.Nickname}. Relative position inside {room}: {RelativePosition}");
                        ev.Player.SendHint($"Relative position inside {room}: {RelativePosition}. This was also sent to the console.", 6f);
                        ev.Player.SendConsoleMessage($"Relative position inside {room}: {RelativePosition}", "white");
                        Vector3 Scale = new(0.2f, 0.2f, 0.2f);
                        PrimitiveObjectToy primitive = PrimitiveObjectToy.Create(hitInfo.point);
                        primitive.Type = PrimitiveType.Cube;
                        primitive.Color = color;
                        primitive.Scale = Scale;
                        primitive.Flags = PrimitiveFlags.Visible;
                        primitive.Rotation = ev.Player.Room.Rotation;
                        primitive.GameObject.name = $"UCI {RelativePosition}";
                        ToolGunPrimitives.TryAdd(primitive, ev.Player.PlayerId);
                    }
                }
            }
        }
        
        public void Receivingeffect(PlayerEffectUpdatingEventArgs ev)
        {
            if (ev.Effect == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                LogManager.Debug($"{ev.Player.Nickname} is reciving {ev.Effect}.");
                ISCP207Data SCP207Data = CustomItem.CustomItem.CustomData as ISCP207Data;
                if (ev.Effect.GetType() == typeof(CustomPlayerEffects.Scp207) || ev.Effect.GetType() == typeof(CustomPlayerEffects.AntiScp207))
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
                    DestroyLightOnPickup(ev.Pickup);
                else
                    LogManager.Error($"Couldnt destroy light on {ev.Pickup.Type}.");
            }
        }

        // Debugging Events.
        /// <summary>
        /// The debugging event for dropping a <see cref="Item"/>
        /// </summary>
        public void Ondrop(PlayerDroppingItemEventArgs ev)
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
        public void OnDebuggingpickup(PlayerPickedUpItemEventArgs ev)
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
        public void Onuse(PlayerUsingItemEventArgs ev)
        {
            if (ev.UsableItem == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.UsableItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.UsableItem.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is using {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for reloading a <see cref="Firearm"/>
        /// </summary>
        public void Onreloading(PlayerReloadingWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.FirearmItem.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is reloading {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for shooting a <see cref="Firearm"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onshooting(PlayerShootingWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.FirearmItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.FirearmItem.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is shooting {CustomItem.CustomItem.Name}");
            }
            else return;
        }
        /// <summary>
        /// The debugging event for throwing a <see cref="Throwable"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onthrown(PlayerThrewProjectileEventArgs ev)
        {
            if (ev.Projectile == null)
                return;
                
            if (Utilities.TryGetSummonedCustomItem(ev.Projectile.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Projectile.Serial == CustomItem.Serial)
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
                ActiveLights.TryRemove(Pickup);
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