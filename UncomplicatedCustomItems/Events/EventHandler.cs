using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API.Features;
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
using PlayerRoles;
using UncomplicatedCustomItems.Enums;
using System.Linq;
using System;
using UserSettings.ServerSpecific;
using Exiled.API.Features;
using Exiled.API.Extensions;
using LabApi.Events.Arguments.PlayerEvents;
using UncomplicatedCustomItems.Events.Methods;
using UncomplicatedCustomItems.Extensions;
using LABAPI = LabApi.Features.Wrappers;
using UncomplicatedCustomItems.Interfaces;
using Exiled.Events.EventArgs.Scp914;
using Exiled.API.Features.Core.UserSettings;
using System.Globalization;
using Exiled.Events.EventArgs.Server;

namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public Dictionary<Pickup, Light> ActiveLights = [];
        /// <summary>
        /// The <see cref="Vector3"/> coordinates of the latest detonation point for a <see cref="Exiled.API.Features.Pickups.Projectiles.EffectGrenadeProjectile"/>.
        /// Triggered by the <see cref="GrenadeExploding"/> method.
        /// </summary>
        public Vector3 DetonationPosition { get; set; } = Vector3.zero;
        private bool ChargeAttack { get; set; } = false;
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that stores appearances for the <see cref="CustomFlags.Disguise"/> and <see cref="CustomFlags.ChangeAppearanceOnKill"/> flags.
        /// Triggered by the <see cref="OnDying"/> method.
        /// </summary>
        public static Dictionary<int, RoleTypeId> Appearance = [];
        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> that handles equiped keycards 
        /// Triggered by the <see cref="OnChangedItem"/> method.
        /// </summary>
        public static Dictionary<ushort, SummonedCustomItem> EquipedKeycards = [];
        private static Dictionary<Player, CoroutineHandle> _relativePosCoroutine = [];

        internal static IEnumerable<SettingBase> _ToolGunSettings;

        internal static Dictionary<Primitive, int> ToolGunPrimitives = [];

        public void OnHurt(HurtEventArgs ev)
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
        public void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || !ev.IsAllowed)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.DoNotTriggerTeslaGates))
                ev.IsTriggerable = false;
            else return;
        }

        public void OnShooting(ShootingEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.Item == null || ev.Firearm == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (ev.Firearm.Aiming)
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                ev.Firearm.Inaccuracy = data.AimingInaccuracy;
            }
            else
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                ev.Firearm.Inaccuracy = data.Inaccuracy;
            }
            if (customItem.HasModule(CustomFlags.InfiniteAmmo))
            {
                IWeaponData data = customItem.CustomItem.CustomData as IWeaponData;
                ev.Firearm.MagazineAmmo = data.MaxMagazineAmmo;
                LogManager.Debug($"InfiniteAmmo flag was triggered: magazine refilled to {data.MaxMagazineAmmo}"); // This will spam the console if debug is enabled and a customitem has the infinite ammo flag.
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

        public void OnItemUse(UsingItemCompletedEventArgs ev)
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

        public void OnChangedItem(ChangedItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost)
                return;
            if (ev.Item is not null)
            {
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
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                    EquipedKeycards.TryAdd(CustomItem.Serial, CustomItem);
                if (CustomItem.HasModule(CustomFlags.ToolGun))
                {
                    _ToolGunSettings = new SettingBase[]
                    {
                        new HeaderSetting("UCI ToolGun Settings", hintDescription: "If multiple are created the first one will be the correct one"),
                        new UserTextInputSetting(21, "Primitive Color", placeHolder: "255, 0, 0, -1",  hintDescription: "The color of the primitives spawned by the ToolGun"),
                        new TwoButtonsSetting(22, "Deletion Mode", "ADS", "FlashLight Toggle", hintDescription: "Sets the deletion mode of the ToolGun"),
                        new TwoButtonsSetting(23, "Delete Primitives when unequipped?", "Yes", "No")
                    };
                    SettingBase.Register(_ToolGunSettings, p => p.Id == ev.Player.Id);
                    StartRelativePosCoroutine(ev.Player);
                }
            }
            if (ev.OldItem != null)
            {
                if (Utilities.TryGetSummonedCustomItem(ev.OldItem.Serial, out SummonedCustomItem customItem))
                    if (customItem.HasModule(CustomFlags.ToolGun))
                    {
                        SSTwoButtonsSetting clearList = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 23);
                        foreach (Primitive primitive in Primitive.List.ToList())
                            if (ToolGunPrimitives.TryGetValue(primitive, out int iD))
                                if (clearList.SyncIsA)
                                    if (ev.Player.Id == iD)
                                        primitive.Destroy();
                        SettingBase.Unregister(p => p.Id == ev.Player.Id, _ToolGunSettings);
                    }
            }
        }

        public void Onpickup(ItemAddedEventArgs ev)
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
        }

        public void OnRoundEnd(EndingRoundEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            foreach (Player player in Player.List)
            {
                SettingBase.Unregister(player, _ToolGunSettings);
            }
        }

        public void GrenadeExploding(ExplodingGrenadeEventArgs ev)
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
                                LABAPI.Scp244Pickup scp244Pickup = (LABAPI.Scp244Pickup)LABAPI.Scp244Pickup.Create((ItemType)SpawnItemWhenDetonatedSettings.ItemId, ev.Position);
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
                                LABAPI.Pickup pickup = LABAPI.Pickup.Create((ItemType)SpawnItemWhenDetonatedSettings.ItemId, ev.Position);
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
                                LABAPI.ExplosiveGrenadeProjectile grenade = (LABAPI.ExplosiveGrenadeProjectile)LABAPI.ExplosiveGrenadeProjectile.SpawnActive(ClusterOffset(ev.Position), ClusterSettings.ItemToSpawn, ev.Player, (double)ClusterSettings.FuseTime);
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
                                LABAPI.Pickup pickup = LABAPI.Pickup.Create(ClusterSettings.ItemToSpawn, ClusterOffset(ev.Position), ev.Player.Rotation, Scale);
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

        public void OnPickupUpgrade(UpgradingPickupEventArgs ev)
        {
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
                                        new SummonedCustomItem(customItem, ev.OutputPosition);
                                        LogManager.Debug($"{nameof(OnPickupUpgrade)}: CustomItem created successfully at {ev.OutputPosition}");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnPickupUpgrade)}: Error creating CustomItem: {ex.Message}");
                                    }
                                    try
                                    {
                                        ev.Pickup.Destroy();
                                        LogManager.Debug($"{nameof(OnPickupUpgrade)}: Original pickup destroyed successfully");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnPickupUpgrade)}: Error destroying pickup: {ex.Message}");
                                    }
                                }
                                else
                                    LogManager.Debug($"{nameof(OnPickupUpgrade)}: {ev.KnobSetting} != {craftableSettings.KnobSetting} or {ev.Pickup} != {craftableSettings.OriginalItem}");
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"{nameof(OnPickupUpgrade)}: Exception: {ex.Message}");
                            }
                        }
                    }
                }    
            }
        }
        
        public void OnItemUpgrade(UpgradingInventoryItemEventArgs ev)
        {
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
                                    LogManager.Debug($"{nameof(OnItemUpgrade)}: Giving {customItem.Name} to {ev.Player.DisplayNickname}...");
                                    try
                                    {
                                        ev.Player.RemoveItem(ev.Item);
                                        new SummonedCustomItem(customItem, ev.Player);
                                        LogManager.Debug($"{nameof(OnItemUpgrade)}: Gave {customItem.Name} to {ev.Player.DisplayNickname}...");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogManager.Error($"{nameof(OnItemUpgrade)}: Error creating CustomItem: {ex.Message}");
                                    }
                                }
                                else
                                    LogManager.Debug($"{nameof(OnItemUpgrade)}: {ev.KnobSetting} != {craftableSettings.KnobSetting} or {ev.Item} != {craftableSettings.OriginalItem}");
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"{nameof(OnItemUpgrade)}: Exception: {ex.Message}");
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
                LogManager.Debug($"Destroyed pickup. Type: {Pickup.Type} Previous owner: {Pickup.PreviousOwner} Serial: {Pickup.Serial}");
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
                    player.ShowHint($"<voffset=-19em>{player.CurrentRoom.Type} - {player.CurrentRoom.LocalPosition(player.Position)}</voffset>");
                    yield return Timing.WaitForSeconds(0.1f);
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
            if (settingBase is not SSKeybindSetting keybindSetting || keybindSetting.SettingId != 20 || !keybindSetting.SyncIsPressed)
                return;
            if (!Player.TryGet(referenceHub, out Player player))
                return;

            if (player.CurrentItem is null)
            {
                foreach (Item item in player.Items)
                {
                    if (item.IsArmor)
                    {
                        if (Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem))
                        {
                            if (!player.IsConnected || player.IsInventoryEmpty)
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

        public void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null)
                return;
            if (ev.Player == null)
                return;
            if (ev.Attacker.CurrentItem == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Jailbird)
                {
                    IJailbirdData Data = CustomItem.CustomItem.CustomData as IJailbirdData;
                    if (!ChargeAttack)
                        ev.Amount = Data.MeleeDamage;
                    else
                    {
                        ev.Amount = Data.ChargeDamage;
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

        public void OnSpawned(SpawnedEventArgs ev)
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
                }
            });
        }

        public void OnDoorInteracting(InteractingDoorEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.CanInteract == false)
                return;
            if (ev.Door.KeycardPermissions == KeycardPermissions.None)
                return;
            if (ev.Player.CurrentItem == null)
                return;
            // This probably will throw a error with plugins like RemoteKeycard
            if (Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out SummonedCustomItem CustomItem))
            {
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Keycard)
                {
                    IKeycardData Data = CustomItem.CustomItem.CustomData as IKeycardData;
                    Timing.CallDelayed(0.2f, () =>
                    {
                        if (ev.Door.IsOpen)
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
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
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
        public void OnLockerInteracting(InteractingLockerEventArgs ev)
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
                CustomItem.HandleEvent(ev.Player, ItemEvents.Use, ev.LightItem.Serial);
            SwitchRoleOnUseMethod.Start(CustomItem, ev.Player);
        }
        public void ThrownProjectile(ThrownProjectileEventArgs ev)
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
        public void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
        {
            if (ev.Item == null || ev.Player == null || ev.Firearm == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;

            if (customItem.HasModule(CustomFlags.WorkstationBan))
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint.Replace("%name%", customItem.CustomItem.Name), Plugin.Instance.Config.WorkstationBanHintDuration);
            }
            else return;
        }
        public void OnWorkstationActivation(ActivatingWorkstationEventArgs ev)
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
        public void OnDeath(SummonedCustomItem customItem)
        {
            if (customItem.CustomItem.CustomFlags.HasValue && customItem.HasModule(CustomFlags.ItemGlow))
            {
                foreach (ItemGlowSettings ItemGlowSettings in customItem.CustomItem.FlagSettings.ItemGlowSettings)
                {
                    LogManager.Debug("SpawnLightOnItem method triggered");

                    if (customItem.Pickup?.Base?.gameObject == null)
                        return;

                    GameObject itemGameObject = customItem.Pickup.Base.gameObject;
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

                    var light = Light.Create(customItem.Pickup.Position);
                    light.Color = lightColor;
                    light.Intensity = 0.7f;
                    light.Range = 0.5f;
                    light.ShadowType = LightShadows.None;

                    light.Base.gameObject.transform.SetParent(itemGameObject.transform, true);
                    LogManager.Debug($"Item Light spawned at position: {light.Base.transform.position}");

                    ActiveLights[customItem.Pickup] = light;
                }
            }
        }
        public void OnDrop(DroppedItemEventArgs ev)
        {
            if (ev.Pickup == null)
                return;

            if (Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem SummonedCustomItem))
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
                if (SummonedCustomItem.CustomItem.CustomFlags.HasValue && SummonedCustomItem.HasModule(CustomFlags.Disguise))
                {
                    ev.Player.ChangeAppearance(ev.Player.Role);
                }
            }

            if (!Utilities.TryGetSummonedCustomItem(ev.Pickup.Serial, out SummonedCustomItem customItem) || !customItem.CustomItem.CustomFlags.HasValue)
                return;
            if (customItem.HasModule(CustomFlags.ItemGlow))
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
            if (customItem.HasModule(CustomFlags.DieOnDrop))
            {
                foreach (DieOnDropSettings DieOnDropSettings in customItem.CustomItem.FlagSettings.DieOnDropSettings)
                {
                    LogManager.Debug($"Checking Vaporize setting for {customItem.CustomItem.Name}");
                    if (DieOnDropSettings.Vaporize != null && (bool)DieOnDropSettings.Vaporize)
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                            LogManager.Debug($"{ev.Player.Nickname} is being vaporized by {customItem.CustomItem.Name}");
                            ev.Player.Vaporize();
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Vaporize {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                    else
                        LogManager.Debug($"Vaporize settings were null or false for {customItem.CustomItem.Name}");
                    if (DieOnDropSettings.DeathMessage.Count() >= 1 && DieOnDropSettings.DeathMessage != null)
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                            ev.Player.Kill($"{DieOnDropSettings.DeathMessage.Replace("%name%", customItem.CustomItem.Name)}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Kill {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                    else
                    {
                        try
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                            ev.Player.Kill($"Killed by {customItem.CustomItem.Name}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Silent("Name | Id | CustomFlag(s)");
                            LogManager.Silent($"{customItem.CustomItem.Name} - {customItem.CustomItem.Id} - {customItem.CustomItem.CustomFlags}");
                            LogManager.Error($"Couldnt Kill {ev.Player.DisplayNickname}\n Error: {ex.Message}\n Code: {ex.HResult}\n Please send this in the bug-report forum in our Discord!");
                        }
                    }
                }
            }
            else return;
        }
        public void OnDropping(DroppingItemEventArgs ev)
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

        public void OnDying(DyingEventArgs ev)
        {
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
            else return;
        }
        public void OnVerified(VerifiedEventArgs ev)
        {
            foreach (var entry in Appearance)
            {
                LogManager.Debug($"{nameof(OnVerified)}: Changing {entry.Key} appearance to {entry.Value}");
                int playerId = entry.Key;
                Player.TryGet(playerId, out Player player);
                RoleTypeId roleID = entry.Value;
                player.ChangeAppearance(roleID);
            }
        }
        public void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.IsHost)
                return;
                
            if (Appearance.ContainsKey(ev.Player.Id))
            {
                LogManager.Debug($"{nameof(OnVerified)}: Removing {ev.Player.Id} from appearance dictionary");
                Appearance.TryRemove(ev.Player.Id);
            }
        }

        public void OnShot(ShotEventArgs ev)
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
                    LABAPI.ExplosiveGrenadeProjectile grenade = (LABAPI.ExplosiveGrenadeProjectile)LABAPI.ExplosiveGrenadeProjectile.SpawnActive(ev.Position, ItemType.GrenadeHE, ev.Player, 0.2);
                    grenade.MaxRadius = ExplosiveBulletsSettings.DamageRadius ?? 10f;
                    grenade.FuseEnd();
                }
            }
            if (customItem.HasModule(CustomFlags.ToolGun))
            {
                SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(ev.Player.ReferenceHub, 22);
                if (deletionMode.SyncIsA && ev.Firearm.Aiming)
                {
                    foreach (Primitive primitive in Primitive.List.ToList())
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
                    foreach (Primitive primitive in Primitive.List.ToList())
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
                    LogManager.Info($"Triggered by {ev.Player.DisplayNickname}. Relative position inside {ev.Player.CurrentRoom.Name}: {RelativePosition}");
                    ev.Player.ShowHint($"Relative position inside {ev.Player.CurrentRoom.Type}: {RelativePosition}. This was also sent to the console.", 6f);
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

        public void OnCharge(ChargingJailbirdEventArgs ev)
        {
            if (ev.Player == null || ev.Player.CurrentItem == null || ev.Player == null)
                return;
            if (!Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem) || !CustomItem.CustomItem.CustomFlags.HasValue)
                return;

            if (CustomItem.HasModule(CustomFlags.NoCharge))
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
                ChargeAttack = true;
            if (CustomItem.HasModule(CustomFlags.EffectWhenUsed))
            {
                AudioApi AudioApi = new();
                if (ev.Item != null)
                {
                    LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {CustomItem.CustomItem.Name}.");
                    AudioApi.PlayAudio(CustomItem, ev.Player.Position);
                }
            }
            else return;
        }
        
        public void Receivingeffect(ReceivingEffectEventArgs ev)
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
        public void OnDebuggingpickup(ItemAddedEventArgs ev)
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