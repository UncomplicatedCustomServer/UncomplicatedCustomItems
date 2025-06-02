using Exiled.API.Enums;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Modules.Scp127;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Events.Methods;
using UnityEngine;
using UserSettings.ServerSpecific;
using LabApiWrappers = LabApi.Features.Wrappers;
using Exiled.API.Extensions;
using LabApi.Events.Arguments.PlayerEvents;
using UncomplicatedCustomItems.API.Extensions;
using System.Globalization;
using Exiled.API.Features.Items;
using CustomPlayerEffects;
using Exiled.API.Features;

namespace UncomplicatedCustomItems.Events
{
    internal class PlayerHandler
    {
        internal static Dictionary<int, LabApiWrappers.CapybaraToy> Capybara = [];
        internal static Dictionary<Player, CoroutineHandle> _HumeShieldRegenCoroutine = [];
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that stores appearances for the <see cref="CustomFlags.Disguise"/> and <see cref="CustomFlags.ChangeAppearanceOnKill"/> flags.
        /// </summary>
        public static Dictionary<int, RoleTypeId> Appearance = [];
        internal static bool ChargeAttack { get; set; } = false;
        internal static Dictionary<Player, long> _damageTimes = [];
        internal static Dictionary<Player, CoroutineHandle> _relativePosCoroutine = [];
        internal static Dictionary<Primitive, int> ToolGunPrimitives = [];

        public static void Register()
        {
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.ItemAdded += Onpickup;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            LabApi.Events.Handlers.PlayerEvents.FlippedCoin += FlippedCoin;
            LabApi.Events.Handlers.PlayerEvents.ToggledFlashlight += ToggledFlashlight;
            Exiled.Events.Handlers.Player.Dying += OnDying;
            Exiled.Events.Handlers.Player.ChangedItem += OnChangedItem;
            Exiled.Events.Handlers.Player.DroppingItem += OnDropping;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.InteractingDoor += OnDoorInteracting;
            Exiled.Events.Handlers.Player.UnlockingGenerator += OnGeneratorUnlock;
            Exiled.Events.Handlers.Player.InteractingLocker += OnLockerInteracting;
            Exiled.Events.Handlers.Player.ReceivingEffect += Receivingeffect;
            Exiled.Events.Handlers.Player.ThrownProjectile += ThrownProjectile;
            Exiled.Events.Handlers.Player.Hurt += OnHurt;
            Exiled.Events.Handlers.Player.TriggeringTesla += OnTriggeringTesla;
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            Exiled.Events.Handlers.Player.UsingItemCompleted += OnItemUse;
            Exiled.Events.Handlers.Player.Shot += OnShot;
            Exiled.Events.Handlers.Player.ActivatingWorkstation += OnWorkstationActivation;
            Exiled.Events.Handlers.Player.DroppedItem += OnDrop;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.ItemAdded -= Onpickup;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            LabApi.Events.Handlers.PlayerEvents.FlippedCoin -= FlippedCoin;
            LabApi.Events.Handlers.PlayerEvents.ToggledFlashlight -= ToggledFlashlight;
            Exiled.Events.Handlers.Player.Dying -= OnDying;
            Exiled.Events.Handlers.Player.ChangedItem -= OnChangedItem;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDropping;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.InteractingDoor -= OnDoorInteracting;
            Exiled.Events.Handlers.Player.UnlockingGenerator -= OnGeneratorUnlock;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnLockerInteracting;
            Exiled.Events.Handlers.Player.ReceivingEffect -= Receivingeffect;
            Exiled.Events.Handlers.Player.ThrownProjectile -= ThrownProjectile;
            Exiled.Events.Handlers.Player.Hurt -= OnHurt;
            Exiled.Events.Handlers.Player.TriggeringTesla -= OnTriggeringTesla;
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            Exiled.Events.Handlers.Player.UsingItemCompleted -= OnItemUse;
            Exiled.Events.Handlers.Player.Shot -= OnShot;
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnWorkstationActivation;
            Exiled.Events.Handlers.Player.DroppedItem -= OnDrop;
        }

        public static void OnHurt(HurtEventArgs ev)
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
        }
        public static void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || !ev.IsAllowed)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.DoNotTriggerTeslaGates))
                ev.IsTriggerable = false;
            else return;
        }

        public static void OnShooting(ShootingEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.Item == null || ev.Firearm == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem))
                return;

            if (ev.Firearm.Aiming)
            {
                if (ev.Firearm.Type != ItemType.GunSCP127)
                {
                    IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                    ev.Firearm.Inaccuracy = data.AimingInaccuracy;
                }
                else
                {
                    ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                    ev.Firearm.Inaccuracy = data.AimingInaccuracy;
                }
            }
            else if (ev.Firearm.Type != ItemType.GunSCP127)
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                ev.Firearm.Inaccuracy = data.Inaccuracy;
            }
            else
            {
                ISCP127Data data = customItem.CustomItem.CustomData as ISCP127Data;
                ev.Firearm.Inaccuracy = data.Inaccuracy;
            }

            if (!customItem.CustomItem.CustomFlags.HasValue || customItem.HasModule(CustomFlags.None))
                return;

            if (customItem.HasModule(CustomFlags.InfiniteAmmo))
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                ev.Firearm.MagazineAmmo = data.MaxMagazineAmmo;
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
                        ev.Player.Vaporize(ev.Player);
                    if (DieOnUseSettings.DeathMessage != null)
                        ev.Player.Kill($"{DieOnUseSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                    else
                        ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                }
                LogManager.Debug($"DieOnUse triggered: {ev.Player.Nickname} killed.");
            }
        }

        public static void OnItemUse(UsingItemCompletedEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Item == null)
                return;
            if (ev.Usable == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.DieOnUse))
            {
                foreach (DieOnUseSettings DieOnUseSettings in customItem.CustomItem.FlagSettings.DieOnUseSettings)
                {
                    if (DieOnUseSettings.Vaporize ?? false)
                        ev.Player.Vaporize(ev.Player);
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
                            if (EffectSettings.Effect.ToString() == string.Empty)
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
                            EffectType Effect = EffectSettings.Effect;
                            float Duration = EffectSettings.EffectDuration;
                            byte Intensity = EffectSettings.EffectIntensity;
                            ev.Player.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
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

            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                ISCP500Data SCP500Data = CustomItem.CustomItem.CustomData as ISCP500Data;
                ISCP207Data SCP207Data = CustomItem.CustomItem.CustomData as ISCP207Data;
                ISCP1853Data SCP1853Data = CustomItem.CustomItem.CustomData as ISCP1853Data;
                ISCP1576Data SCP1576Data = CustomItem.CustomItem.CustomData as ISCP1576Data;
                if (ev.Item.Type == ItemType.SCP500)
                {
                    if (SCP500Data.Effect.ToString() == string.Empty)
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
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP500Data.Effect} at intensity {SCP500Data.Intensity}, duration is {SCP500Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP500Data.Effect;
                    float Duration = SCP500Data.Duration;
                    byte Intensity = SCP500Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP207 || ev.Item.Type == ItemType.AntiSCP207)
                {
                    if (SCP207Data.Effect.ToString() == string.Empty)
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
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP207Data.Effect} at intensity {SCP207Data.Intensity}, duration is {SCP207Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP207Data.Effect;
                    float Duration = SCP207Data.Duration;
                    byte Intensity = SCP207Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP1853)
                {
                    if (SCP1853Data.Effect.ToString() == string.Empty)
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
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP1853Data.Effect} at intensity {SCP1853Data.Intensity}, duration is {SCP1853Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP1853Data.Effect;
                    float Duration = SCP1853Data.Duration;
                    byte Intensity = SCP1853Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP1576)
                {
                    if (SCP1576Data.Effect.ToString() == string.Empty)
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
                    LogManager.Debug($"{nameof(OnItemUse)}: Applying effect {SCP1576Data.Effect} at intensity {SCP1576Data.Intensity}, duration is {SCP1576Data.Duration} to {ev.Player.DisplayNickname}");
                    EffectType Effect = SCP1576Data.Effect;
                    float Duration = SCP1576Data.Duration;
                    byte Intensity = SCP1576Data.Intensity;
                    ev.Player?.EnableEffect(Effect, Intensity, Duration, true);
                }
                if (ev.Item.Type == ItemType.SCP207 || ev.Item.Type == ItemType.AntiSCP207)
                    if (SCP207Data.RemoveItemAfterUse == false)
                        new SummonedCustomItem(CustomItem.CustomItem, ev.Player);
                if (ev.Item.Type == ItemType.SCP1853)
                    if (SCP1853Data.RemoveItemAfterUse == false)
                        new SummonedCustomItem(CustomItem.CustomItem, ev.Player);
                if (CustomItem.Item.Type == ItemType.Adrenaline || CustomItem.Item.Type == ItemType.Medkit || CustomItem.Item.Type == ItemType.Painkillers)
                    CustomItem.HandleCustomAction(CustomItem.Item);
            }
        }

        public static void OnChangedItem(ChangedItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost)
                return;
            if (ev.Item is not null)
            {
                if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
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
                                if (EffectSettings.Effect.ToString() == string.Empty)
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
                                EffectType Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Player.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"{nameof(OnChangedItem)}: No FlagSettings found on {CustomItem.CustomItem.Name}");
                        }
                    }
                }
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
                        foreach (Primitive primitive in AdminToy.List.OfType<Primitive>().ToList())
                            if (ToolGunPrimitives.TryGetValue(primitive, out int iD))
                                if (clearList.SyncIsA)
                                    if (ev.Player.Id == iD)
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
            if (_HumeShieldRegenCoroutine.TryGetValue(player, out CoroutineHandle handle))
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

            for (; ; )
            {
                if (_damageTimes.TryGetValue(player, out long time))
                {
                    long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
                    player.HumeShieldRegenerationMultiplier = (elapsed >= damagePause) ? regenRate : 0f;
                }
                else
                {
                    player.HumeShieldRegenerationMultiplier = regenRate;
                }

                if (player.CurrentItem == null || player.CurrentItem.Serial != customItem.Serial)
                    yield break;

                yield return Timing.WaitForOneFrame;
            }
        }

        public static void Onpickup(ItemAddedEventArgs ev)
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
                            if (EffectSettings.Effect.ToString() == string.Empty)
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
                            ev.Player.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                    }
                }
            }
            if (CustomItem.HasModule(CustomFlags.Disguise))
            {
                foreach (DisguiseSettings DisguiseSettings in CustomItem.CustomItem.FlagSettings.DisguiseSettings)
                {
                    if (DisguiseSettings.RoleId == null)
                        return;
                    if (DisguiseSettings.DisguiseMessage == null)
                        return;

                    LogManager.Debug($"{nameof(Onpickup)}: Changing {ev.Player.DisplayNickname} appearance to {DisguiseSettings.RoleId}");
                    ev.Player.ChangeAppearance((RoleTypeId)DisguiseSettings.RoleId);
                    ev.Player.Broadcast(10, $"{DisguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                    LogManager.Debug($"{nameof(Onpickup)}: Adding or updating {ev.Player.Id} to appearance dictionary");
                    Appearance.TryAdd(ev.Player.Id, (RoleTypeId)DisguiseSettings.RoleId);
                }
            }
            if (CustomItem.HasModule(CustomFlags.Capybara))
            {
                LabApiWrappers.CapybaraToy capybara = LabApiWrappers.CapybaraToy.Create(ev.Player.Transform);
                capybara.CollidersEnabled = false;
                capybara.Position = capybara.Position + new Vector3(0, -0.8f, 0);
                ev.Player.Scale = new(0.2f, 0.3f, 0.5f);
                capybara.Scale = new(6f, 4f, 2.8f);
                Capybara.TryAdd(ev.Player.Id, capybara);
            }
        }
        public static void OnWorkstationActivation(ActivatingWorkstationEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.WorkstationBan))
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
            }
            else return;
        }

        public static void OnDrop(DroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem SummonedCustomItem))
                return;
            if (SummonedCustomItem.HasModule(CustomFlags.Disguise))
                ev.Player.ChangeAppearance(ev.Player.Role);

            if (SummonedCustomItem.HasModule(CustomFlags.Capybara))
            {
                if (Capybara.TryGetValue(ev.Player.Id, out LabApiWrappers.CapybaraToy capybara))
                {
                    capybara.Destroy();
                    ev.Player.Scale = new(1, 1, 1);
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
                            ev.Player.Vaporize();
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{SummonedCustomItem.CustomItem.Name} - {SummonedCustomItem.CustomItem.Id} - {SummonedCustomItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Vaporize {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
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
                            LogManager.Error($"Couldnt Kill {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
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
                            LogManager.Error($"Couldnt Kill {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                }
            }
            else return;
        }
        public static void FlippedCoin(PlayerFlippedCoinEventArgs ev)
        {
            if (ev.CoinItem == null || ev.Player == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.CoinItem.Serial, out SummonedCustomItem CustomItem))
                CustomItem.HandleEvent(ev.Player, ItemEvents.Use, ev.CoinItem.Serial);
            SwitchRoleOnUseMethod.Start(CustomItem, ev.Player);
        }
        public static void ToggledFlashlight(PlayerToggledFlashlightEventArgs ev)
        {
            if (ev.LightItem == null || ev.Player == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.LightItem.Serial, out SummonedCustomItem CustomItem))
                CustomItem.HandleEvent(ev.Player, ItemEvents.Use, ev.LightItem.Serial);
            SwitchRoleOnUseMethod.Start(CustomItem, ev.Player);
        }
        public static void ThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (ev.Item == null || ev.Player == null || ev.Projectile == null)
                return;
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;

            if (CustomItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                foreach (EffectSettings EffectSettings in CustomItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (EffectSettings.EffectEvent != null)
                    {
                        if (EffectSettings.EffectEvent == "EffectWhenUsed")
                        {
                            if (EffectSettings.Effect.ToString() == string.Empty)
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
                            ev.Player.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                        }
                    }
                    else
                    {
                        LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                    }
                }
            }
        }
        public static void OnDoorInteracting(InteractingDoorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.CanInteract == false)
                return;
            if (ev.Door.KeycardPermissions == KeycardPermissions.None)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            if (!ev.IsAllowed)
                return;
            if (ev.Door == null)
                return;
            // This probably will throw a error with plugins like RemoteKeycard
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData Data = CustomItem.CustomItem.CustomData as IKeycardData;
                    Timing.CallDelayed(0.1f, () =>
                    {
                        if (ev.Player.HasKeycardPermission((KeycardPermissions)ev.Door.RequiredPermissions))
                        {
                            if (Data.OneTimeUse)
                            {
                                Timing.CallDelayed(0.5f, () =>
                                {
                                    ev.Player.ShowHint($"{Data.OneTimeUseHint.Replace("%name%", CustomItem.CustomItem.Name)}", 8f);
                                    LogManager.Debug($"OneTimeUse is true removing {CustomItem.CustomItem.Name}...");
                                    ev.Player.RemoveItem(CustomItem.Item, true);
                                });
                            }
                        }
                    });
                }
            }
        }
        public static void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
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
                            ev.Player.ShowHint($"{Data.OneTimeUseHint.Replace("%name%", CustomItem.CustomItem.Name)}", 8f);
                            LogManager.Debug($"OneTimeUse is true removing {CustomItem.CustomItem.Name}...");
                            ev.Player.RemoveItem(CustomItem.Item, true);
                        });
                    }
                }
            }
        }
        public static void OnLockerInteracting(InteractingLockerEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            // This probably will throw a error with plugins like RemoteKeycard
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    if (ev.Player.HasKeycardPermission(ev.InteractingChamber.RequiredPermissions))
                    {
                        IKeycardData Data = CustomItem.CustomItem.CustomData as IKeycardData;
                        if (Data.OneTimeUse)
                        {
                            Timing.CallDelayed(0.5f, () =>
                            {
                                ev.Player.ShowHint($"{Data.OneTimeUseHint.Replace("%name%", CustomItem.CustomItem.Name)}", 8f);
                                LogManager.Debug($"OneTimeUse is true removing {CustomItem.CustomItem.Name}...");
                                ev.Player.RemoveItem(CustomItem.Item, true);
                            });
                        }
                    }
                }
            }
        }
        public static void OnSpawned(SpawnedEventArgs ev)
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
                                    ev.Player.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                                }
                            }
                            else
                            {
                                LogManager.Error($"No FlagSettings found on {CustomItem.CustomItem.Name}");
                            }
                        }
                    }
                    if (CustomItem.HasModule(CustomFlags.Disguise))
                    {
                        foreach (DisguiseSettings DisguiseSettings in CustomItem.CustomItem.FlagSettings.DisguiseSettings)
                        {
                            if (DisguiseSettings.RoleId == null)
                                return;
                            if (DisguiseSettings.DisguiseMessage == null)
                                return;

                            LogManager.Debug($"{nameof(OnSpawned)}: Changing {ev.Player.DisplayNickname} appearance to {DisguiseSettings.RoleId}");
                            ev.Player.ChangeAppearance((RoleTypeId)DisguiseSettings.RoleId);
                            ev.Player.Broadcast(10, $"{DisguiseSettings.DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                            LogManager.Debug($"{nameof(OnSpawned)}: Adding or updating {ev.Player.Id} to appearance dictionary");
                            Appearance.TryAdd(ev.Player.Id, (RoleTypeId)DisguiseSettings.RoleId);
                        }
                    }
                    if (CustomItem.HasModule(CustomFlags.Capybara))
                    {
                        LabApiWrappers.CapybaraToy capybara = LabApiWrappers.CapybaraToy.Create(ev.Player.Transform);
                        capybara.CollidersEnabled = false;
                        capybara.Position = capybara.Position + new Vector3(0, -0.8f, 0);
                        ev.Player.Scale = new(0.2f, 0.3f, 0.5f);
                        capybara.Scale = new(6f, 4f, 2.8f);
                        Capybara.TryAdd(ev.Player.Id, capybara);
                    }
                }
            });
        }
        public static void OnDropping(DroppingItemEventArgs ev)
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
                                LogManager.Debug($"Sending CantDrop Hint to {ev.Player.DisplayNickname}\nHint: {CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}");
                                ev.Player.ShowHint($"{CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}", (ushort)CantDropSettings.Duration);
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} - {CustomItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Hint to {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
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
                                LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.DisplayNickname}\nBroadcast: {CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}");
                                ev.Player.Broadcast((ushort)CantDropSettings.Duration, $"{CantDropSettings.Message.Replace("%name%", CustomItem.CustomItem.Name)}", Broadcast.BroadcastFlags.Normal, true);
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Silent("Name | Id | CustomFlag(s)");
                                LogManager.Silent($"{CustomItem.CustomItem.Name} - {CustomItem.CustomItem.Id} - {CustomItem.CustomItem.CustomFlags}");
                                LogManager.Error($"Couldnt send CantDrop Broadcast to {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
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

        public static void OnDying(DyingEventArgs ev)
        {
            if (!ev.Player.IsInventoryEmpty)
            {
                foreach (Item item in ev.Player.Items)
                {
                    if (Utilities.TryGetSummonedCustomItem(item.Serial, out var sItem))
                    {
                        if (sItem.HasModule(CustomFlags.Capybara))
                        {
                            if (Capybara.TryGetValue(ev.Player.Id, out LabApiWrappers.CapybaraToy capybara))
                            {
                                capybara.Destroy();
                                ev.Player.Scale = new(1, 1, 1);
                            }
                        }
                    }
                }
            }

            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (!ev.Attacker.IsConnected)
                return;
            if (!ev.Player.IsConnected)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;
            if (!ev.Attacker.CurrentItem.IsWeapon)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.VaporizeKills))
            {
                try
                {
                    LogManager.Silent("Name | Id | CustomFlag(s)");
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Debug($"Vaporizing {ev.Player.DisplayNickname}");
                    ev.Player.Vaporize();
                }
                catch (Exception ex)
                {
                    LogManager.Silent("Name | Id | CustomFlag(s)");
                    LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                    LogManager.Error($"Couldnt Vaporize {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                }
            }
            if (customItem.HasModule(CustomFlags.ChangeAppearanceOnKill))
            {
                LogManager.Debug($"{nameof(OnDying)}: Changing {ev.Attacker.DisplayNickname} appearance to {ev.Player.Role.Name}");
                ev.Attacker.ChangeAppearance(ev.Player.Role);
                LogManager.Debug($"{nameof(OnDying)}: Adding {ev.Attacker.Id} to appearance dictionary");
                Appearance.TryAdd(ev.Attacker.Id, ev.Player.Role);
            }
            if (customItem.HasModule(CustomFlags.HealOnKill))
            {
                foreach (HealOnKillSettings healOnKillSettings in customItem.CustomItem.FlagSettings.HealOnKillSettings)
                {
                    LogManager.Debug($"{nameof(OnDying)}: Healing {ev.Attacker.DisplayNickname} by {healOnKillSettings.HealAmount}");
                    if (ev.Attacker.Health != ev.Attacker.MaxHealth)
                        ev.Attacker.Heal(healOnKillSettings.HealAmount ?? 5f);
                    else if (healOnKillSettings.ConvertToAhpIfFull ?? false)
                    {
                        LogManager.Debug($"{nameof(OnDying)}: {ev.Attacker.DisplayNickname} health is full and ConvertToAhpIfFull is true adding {healOnKillSettings.HealAmount} to {ev.Attacker.DisplayNickname} AHP");
                        ev.Attacker.AddAhp(healOnKillSettings.HealAmount ?? 5f);
                    }
                }
            }
        }
        public static void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.UserId == "76561199150506472@steam")
            {
                // Baguetter credit tag
                if (Plugin.Instance.Config.EnableCreditTags)
                {
                    ev.Player.RankName = "💻 UCI Lead Developer";
                    ev.Player.RankColor = "emerald";
                }
                if (Plugin.Instance.IsPrerelease)
                    SSS.AddDebugSettingsToUser(ev.Player.ReferenceHub);
                else
                    SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);
            }
            else
                SSS.SendNormalSettingsToUser(ev.Player.ReferenceHub);

            foreach (KeyValuePair<int, RoleTypeId> entry in Appearance)
            {
                LogManager.Debug($"{nameof(OnVerified)}: Changing {entry.Key} appearance to {entry.Value}");
                Player.TryGet(entry.Key, out Player player);
                player.ChangeAppearance(entry.Value);
            }
        }
        public static void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.IsHost)
                return;

            if (Appearance.ContainsKey(ev.Player.Id))
            {
                LogManager.Debug($"{nameof(OnLeft)}: Removing {ev.Player.Id} from appearance dictionary");
                Appearance.TryRemove(ev.Player.Id);
            }
            if (Capybara.ContainsKey(ev.Player.Id))
                Capybara.TryRemove(ev.Player.Id);
        }

        public static void OnShot(ShotEventArgs ev)
        {
            if (ev.Position == null || ev.Firearm == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.EffectWhenUsed))
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
                                if (EffectSettings.Effect.ToString() == string.Empty)
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
                                ev.Player?.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
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
            if (customItem.HasModule(CustomFlags.ExplosiveBullets))
            {
                foreach (ExplosiveBulletsSettings ExplosiveBulletsSettings in customItem.CustomItem.FlagSettings.ExplosiveBulletsSettings)
                {
                    ev.CanHurt = false;
                    ev.CanSpawnImpactEffects = false;
                    LabApiWrappers.ExplosiveGrenadeProjectile grenade = (LabApiWrappers.ExplosiveGrenadeProjectile)LabApiWrappers.ExplosiveGrenadeProjectile.SpawnActive(ev.Position, ItemType.GrenadeHE, ev.Player, 0.2);
                    grenade.MaxRadius = ExplosiveBulletsSettings.DamageRadius ?? 10f;
                    grenade.FuseEnd();
                }
            }
            if (customItem.HasModule(CustomFlags.ToolGun))
            {
                SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 22);
                if (deletionMode.SyncIsA && ev.Firearm.Aiming)
                {
                    foreach (Primitive primitive in AdminToy.List.OfType<Primitive>().ToList())
                    {
                        if (primitive.GameObject.name.Contains("UCI"))
                        {
                            Vector3 halfSize = primitive.Scale / 2f;
                            Vector3 minBounds = primitive.Position - halfSize;
                            Vector3 maxBounds = primitive.Position + halfSize;

                            if (ev.Position.x >= minBounds.x && ev.Position.x <= maxBounds.x && ev.Position.y >= minBounds.y && ev.Position.y <= maxBounds.y && ev.Position.z >= minBounds.z && ev.Position.z <= maxBounds.z)
                                primitive.Destroy();
                        }
                    }
                }
                else if (deletionMode.SyncIsB && ev.Firearm.FlashlightEnabled)
                {
                    foreach (Primitive primitive in AdminToy.List.OfType<Primitive>().ToList())
                    {
                        if (primitive.GameObject.name.Contains("UCI"))
                        {
                            Vector3 halfSize = primitive.Scale / 2f;
                            Vector3 minBounds = primitive.Position - halfSize;
                            Vector3 maxBounds = primitive.Position + halfSize;

                            if (ev.Position.x >= minBounds.x && ev.Position.x <= maxBounds.x && ev.Position.y >= minBounds.y && ev.Position.y <= maxBounds.y && ev.Position.z >= minBounds.z && ev.Position.z <= maxBounds.z)
                                primitive.Destroy();
                        }
                    }
                }
                else
                {
                    PauseRelativePosCoroutine(ev.Player);
                    ev.CanSpawnImpactEffects = false;
                    ev.CanHurt = false;
                    SSPlaintextSetting setting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(ev.Player.ReferenceHub, 21);
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
                    Vector3 RelativePosition = ev.Player.CurrentRoom.LocalPosition(ev.Position);
                    LogManager.Info($"Triggered by {ev.Player.DisplayNickname}. Relative position inside {ev.Player.CurrentRoom.Type}: {RelativePosition}");
                    ev.Player.ShowHint($"Relative position inside {ev.Player.CurrentRoom.Type}: {RelativePosition}. This was also sent to the console.", 6f);
                    ev.Player.SendConsoleMessage($"Relative position inside {ev.Player.CurrentRoom.Type}: {RelativePosition}", "white");
                    Vector3 Scale = new(0.2f, 0.2f, 0.2f);
                    Primitive primitive = Primitive.Create(ev.Position);
                    primitive.Type = PrimitiveType.Cube;
                    primitive.Color = color;
                    primitive.Scale = Scale;
                    primitive.Collidable = false;
                    primitive.Rotation = ev.Player.CurrentRoom.Rotation;
                    primitive.GameObject.name = $"UCI {RelativePosition}";
                    ToolGunPrimitives.TryAdd(primitive, ev.Player.Id);
                }
            }
            if (customItem.HasModule(CustomFlags.EffectShot))
            {
                foreach (EffectSettings EffectSettings in customItem.CustomItem.FlagSettings.EffectSettings)
                {
                    if (ev.Item != null)
                    {
                        if (EffectSettings.EffectEvent != null)
                        {
                            if (EffectSettings.EffectEvent == "EffectShot")
                            {
                                if (EffectSettings.Effect.ToString() == string.Empty)
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

                                LogManager.Debug($"Applying effect {EffectSettings.Effect} at intensity {EffectSettings.EffectIntensity}, duration is {EffectSettings.EffectDuration} to {ev.Target.DisplayNickname}");
                                EffectType Effect = EffectSettings.Effect;
                                float Duration = EffectSettings.EffectDuration;
                                byte Intensity = EffectSettings.EffectIntensity;
                                ev.Target?.EnableEffect(Effect, Intensity, Duration, EffectSettings.AddDurationIfActive ?? false);
                            }
                        }
                        else
                        {
                            LogManager.Error($"No FlagSettings found on {customItem.CustomItem.Name}");
                        }
                    }
                    else
                    {
                        LogManager.Error($"EffectShot Flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                    }
                }
            }
        }
        public static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;

            _damageTimes.TryAdd(ev.Player, DateTimeOffset.Now.ToUnixTimeMilliseconds());
            if (Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Jailbird)
                {
                    Jailbird jailbird = ev.Attacker.CurrentItem as Jailbird;
                    IJailbirdData data = CustomItem.CustomItem.CustomData as IJailbirdData;
                    if (data.TotalCharges >= 2)
                    {
                        data.TotalCharges -= 1;
                        jailbird.WearState = InventorySystem.Items.Jailbird.JailbirdWearState.Healthy;
                    }
                    else if (data.TotalCharges == 1)
                    {
                        data.TotalCharges -= 1;
                        jailbird.WearState = InventorySystem.Items.Jailbird.JailbirdWearState.AlmostBroken;
                    }
                    else if (data.TotalCharges == 0)
                    {
                        jailbird.WearState = InventorySystem.Items.Jailbird.JailbirdWearState.Broken;
                        jailbird.Break();
                    }

                    if (!ChargeAttack)
                        ev.Amount = data.MeleeDamage;
                    else
                    {
                        ev.Amount = data.ChargeDamage;
                        ChargeAttack = false;
                    }
                }
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Weapon)
                {
                    IWeaponData Data = CustomItem.CustomItem.CustomData as IWeaponData;
                    LogManager.Debug($"Reducing {ev.Player.DisplayNickname} health by {Data.Damage}");
                    ev.Amount = Data.Damage;
                }
            }
        }
        public static void Receivingeffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Effect == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Player.CurrentItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
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
                for (; ; )
                {
                    if (player.CurrentItem == null)
                        StopRelativePosCoroutine(player);
                    else if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
                        if (!CustomItem.HasModule(CustomFlags.ToolGun))
                            StopRelativePosCoroutine(player);
                    if (player.CurrentRoom == null)
                        PauseRelativePosCoroutine(player);

                    SSPlaintextSetting colorSetting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(player.ReferenceHub, 21);
                    SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(player.ReferenceHub, 22);

                    string deletioncolor = string.Empty;
                    bool deletionbool = false;
                    string DeletionMode = string.Empty;
                    if (deletionMode.SyncIsA)
                    {
                        DeletionMode = "ADS";
                        if (player.IsAimingDownWeapon)
                            deletionbool = true;
                        else
                            deletionbool = false;
                    }
                    else if (deletionMode.SyncIsB)
                    {
                        DeletionMode = "Flashlight Toggle";
                        if (player.HasFlashlightModuleEnabled)
                            deletionbool = true;
                        else
                            deletionbool = false;
                    }

                    if (deletionbool)
                        deletioncolor = "#00ff00";
                    else
                        deletioncolor = "#Ff0000";

                    API.Extensions.StringExtensions.TryParseVector3(colorSetting.SyncInputText, out Vector3 color);
                    string hexcolor = Vector3Extensions.ToHexColor(color);
                    string hinttext = $"<pos=-10em><voffset=-12.3em><color={player.RankColor}>{player.DisplayNickname} - {player.Role.Name}</color></voffset>\n<pos=-10em>{player.CurrentRoom.Type} - <color=yellow>{player.CurrentRoom.LocalPosition(player.Position)}</color>\n<pos=-10em>Primitive Color: <color={hexcolor}>{color}</color>\n<pos=-10em>Deletion Mode: {DeletionMode}\n<pos=-10em>Deleting: <color={deletioncolor}>{deletionbool}</color>";
                    player.ShowHint($"<align=left>{hinttext}</align>", 0.5f);
                    yield return Timing.WaitForOneFrame;
                }
            }
            finally
            {
                if (_relativePosCoroutine.ContainsKey(player))
                    _relativePosCoroutine.Remove(player);
            }
        }
    }
}
