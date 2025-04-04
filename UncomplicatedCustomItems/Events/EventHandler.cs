using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomModules;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Extensions;
using UnityEngine;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Light = Exiled.API.Features.Toys.Light;
using Exiled.Events.EventArgs.Server;
using Mirror;
using Exiled.API.Features;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using CustomPlayerEffects;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Events
{
    public class EventHandler
    {
        /// <summary>
        /// The Dictionary that handles lights spawned from the <see cref="OnDrop"/> method.
        /// </summary>
        public Dictionary<Pickup, Light> ActiveLights = [];
        /// <summary>
        /// The <see cref="float"/> that handles how much a <see cref="CustomItem"/> with the LifeSteal <see cref="CustomModule"/>.
        /// </summary>
        public float Amount { get; set; } = 0f;
        /// <summary>
        /// The Percentage that a <see cref="CustomItem"/> with the LifeSteal <see cref="CustomModule"/> will heal at. Determined by doing HealedAmount = Amount * Percentage
        /// </summary>
        public float Percentage = 0.5f;
        
        public void OnHurt(HurtEventArgs ev)
        {
            LogManager.Debug("OnHurt event is being triggered");
            if (ev.Player is not null && ev.Attacker is not null && ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem summonedCustomItem))
            {
                LogManager.Debug("Fuck all is being triggered");
                summonedCustomItem.LastDamageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var flagSettings = SummonedCustomItem.GetAllFlagSettings();
                if (flagSettings != null && flagSettings.Count > 0)
                {
                    var FlagSettings = flagSettings.FirstOrDefault();

                    if (FlagSettings != null)
                    {
                        if (FlagSettings.LifeStealAmount > 0)
                        {
                            Amount = FlagSettings.LifeStealAmount;
                        }
                        else
                        {
                            LogManager.Error($"Invalid LifeStealAmount: {FlagSettings.LifeStealAmount}");
                        }
                    }
                }
                else
                {
                    LogManager.Error("No FlagSettings found on custom item");
                }


                if (flagSettings != null && flagSettings.Count > 0)
                {
                    var flagSetting = flagSettings.FirstOrDefault();

                    if (flagSetting != null)
                    {
                        if (flagSetting.LifeStealPercentage > 0)
                        {
                            Percentage = flagSetting.LifeStealPercentage;
                        }
                        else
                        {
                            LogManager.Error($"Failed to parse: {flagSetting.LifeStealPercentage}");
                        }
                    }
                }
                else
                {
                    LogManager.Error("No FlagSettings found on custom item");
                }

                if (ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<LifeSteal>())
                {
                    LogManager.Debug("LifeSteal custom flag is being triggered");

                    if (Amount > 0)
                    {
                        float HealedAmount = Amount * Percentage;
                        ev.Attacker.Heal(HealedAmount);
                        LogManager.Debug($"LifeSteal custom flag triggered, healed {HealedAmount} HP");
                    }
                }
            }
        }
        public void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Player is not null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<DoNotTriggerTeslaGates>())
                ev.IsTriggerable = false;
            else return;
        }
        public void OnShooting(ShootingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<InfiniteAmmo>())
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
            else if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<CustomSound>())
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
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<DieOnUse>())
            {
                if (ev.Item != null)
                {
                    ev.Player.Kill(DamageType.Custom);
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
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<DieOnUse>())
            {
                if (ev.Item != null)
                {
                    ev.Player.Kill(DamageType.Custom);
                    LogManager.Debug("DieOnUse triggered: user killed.");
                }
                else
                {
                    LogManager.Error($"DieOnUse flag was triggered but couldnt be ran for {customItem.CustomItem.Name}.");
                }
            }
            else if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<EffectWhenUsed>())
            {
                if (ev.Item != null)
                {
                    var flagSettings = SummonedCustomItem.GetAllFlagSettings();

                    if (flagSettings != null && flagSettings.Count > 0)
                    {
                        var flagSetting = flagSettings.FirstOrDefault();

                        if (flagSetting.EffectEvent == "EffectWhenUsed")
                        {
                            if (flagSetting.Effect == null)
                            {
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"Applying effect {flagSetting.Effect} at intensity {flagSetting.EffectIntensity}, duration is {flagSetting.EffectDuration} to {ev.Player}");
                            EffectType Effect = flagSetting.Effect;
                            float Duration = flagSetting.EffectDuration;
                            byte Intensity = flagSetting.EffectIntensity;
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
            else if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem Customitem) && Customitem.HasModule<CustomSound>())
            {
                AudioApi AudioApi = new();
                if (ev.Item != null)
                {
                    LogManager.Debug($"Attempting to play audio at {ev.Player.Position} triggered by {ev.Player.Nickname} using {Customitem.CustomItem.Name}.");
                    AudioApi.PlayAudio(Customitem, ev.Player.Position);
                }
            }
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customitem))
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
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem2))
            {
                ISCP207Data SCP207Data = CustomItem2.CustomItem.CustomData as ISCP207Data;
                if (ev.Item.Type == ItemType.SCP207 || ev.Item.Type == ItemType.AntiSCP207)
                {
                    if (SCP207Data.RemoveItemAfterUse == false)
                    {
                        Server.ExecuteCommand($"/uci give {CustomItem2.CustomItem.Id} {ev.Player.Id}");
                    }
                }
                ISCP1853Data SCP1853Data = CustomItem2.CustomItem.CustomData as ISCP1853Data;
                if (ev.Item.Type == ItemType.SCP1853)
                {
                    if (SCP1853Data.RemoveItemAfterUse == false)
                    {
                        Server.ExecuteCommand($"/uci give {CustomItem2.CustomItem.Id} {ev.Player.Id}");
                    }
                }
            }
            else return;
        }

        public void ThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<EffectWhenUsed>())
            {
                if (ev.Item != null)
                {
                    var flagSettings = SummonedCustomItem.GetAllFlagSettings();

                    if (flagSettings != null && flagSettings.Count > 0)
                    {
                        var flagSetting = flagSettings.FirstOrDefault();

                        if (flagSetting.EffectEvent == "EffectWhenUsed")
                        {
                            if (flagSetting.Effect == null)
                            {
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectDuration < -1)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"Applying effect {flagSetting.Effect} at intensity {flagSetting.EffectIntensity}, duration is {flagSetting.EffectDuration} to {ev.Player}");
                            EffectType Effect = flagSetting.Effect;
                            float Duration = flagSetting.EffectDuration;
                            byte Intensity = flagSetting.EffectIntensity;
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
        public void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
        {

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<WorkstationBan>())
            {
                if (ev.Player != null)
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint, Plugin.Instance.Config.WorkstationBanHintDuration);
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

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<WorkstationBan>())
            {
                if (ev.Player != null)
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint, Plugin.Instance.Config.WorkstationBanHintDuration);
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
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<ItemGlow>())
            {
                if (ev.Player != null)
                {
                    LogManager.Debug("SpawnLightOnItem method triggered");

                    if (ev.Pickup?.Base?.gameObject == null)
                        return;

                    GameObject itemGameObject = ev.Pickup.Base.gameObject;
                    Color lightColor = Color.blue;

                    var FlagSettings = SummonedCustomItem.GetAllFlagSettings();
                    if (FlagSettings != null && FlagSettings.Count > 0)
                    {
                        var flagSetting = FlagSettings.FirstOrDefault();

                        if (flagSetting != null && !string.IsNullOrEmpty(flagSetting.GlowColor))
                        {
                            if (ColorUtility.TryParseHtmlString(flagSetting.GlowColor, out Color parsedColor))
                            {
                                lightColor = parsedColor;
                            }
                            else
                            {
                                LogManager.Error($"Failed to parse color: {flagSetting.GlowColor} for {customItem.CustomItem.Name}");
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
            else return;
        }
        public void OnShot(ShotEventArgs ev)
        {
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<EffectWhenUsed>())
            {
                if (ev.Item != null)
                {
                    var flagSettings = SummonedCustomItem.GetAllFlagSettings();
                    if (flagSettings != null && flagSettings.Count > 0)
                    {
                        var flagSetting = flagSettings.FirstOrDefault();
                        LogManager.Debug($"Checking if {flagSetting.EffectEvent} = EffectWhenUsed");
                        if (flagSetting.EffectEvent == "EffectWhenUsed")
                        {
                            LogManager.Debug($"{flagSetting.EffectEvent} = EffectWhenUsed");
                            if (flagSetting.Effect == null)
                            {
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for ID: {customItem.CustomItem.Id} Name: {customItem.CustomItem.Name}");
                                return;
                            }
                            LogManager.Debug($"Applying effect {flagSetting.Effect} at intensity {flagSetting.EffectIntensity}, duration is {flagSetting.EffectDuration} to {ev.Player}");
                            EffectType Effect = flagSetting.Effect;
                            float Duration = flagSetting.EffectDuration;
                            byte Intensity = flagSetting.EffectIntensity;
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
            else if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<ExplosiveBullets>())
            {
                var flagSettings = SummonedCustomItem.GetAllFlagSettings();
                var flagSetting = flagSettings.FirstOrDefault();
                if (ev.Firearm != null)
                {
                    
                    ev.CanSpawnImpactEffects = false;
                    Vector3 Position = ev.Position;
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    float DamageRadius = flagSetting.DamageRadius ?? 1f;
                    grenade.MaxRadius = DamageRadius;
                    grenade.FuseTime = .01f;
                    grenade.SpawnActive(Position);
                }
            }
            else return;
        }
        public void OnShot2(ShotEventArgs ev)
        {
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<EffectShot>())
            {
                if (ev.Item != null)
                {
                    var flagSettings = SummonedCustomItem.GetAllFlagSettings();
                    if (flagSettings != null && flagSettings.Count > 0)
                    {
                        var flagSetting = flagSettings.FirstOrDefault();
                        if (flagSetting.EffectEvent == "EffectShot")
                        {
                            if (flagSetting.Effect == null)
                            {
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for ID: {CustomItem.CustomItem.Id} Name: {CustomItem.CustomItem.Name}");
                                return;
                            }

                            LogManager.Debug($"Applying effect {flagSetting.Effect} at intensity {flagSetting.EffectIntensity}, duration is {flagSetting.EffectDuration} to {ev.Target.DisplayNickname}");
                            EffectType Effect = flagSetting.Effect;
                            float Duration = flagSetting.EffectDuration;
                            byte Intensity = flagSetting.EffectIntensity;
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
            else return;
        }

        public void OnCharge(ChargingJailbirdEventArgs ev)
        {
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<NoCharge>())
            {
                if (ev.Item != null)
                {
                    ev.IsAllowed = false;
                    ev.Player.CurrentItem = null;
                }
            }
            else if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<EffectWhenUsed>())
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

        public void Onroundend(RoundEndedEventArgs ev)
        {
            Exiled.Events.Handlers.Map.PickupDestroyed -= OnPickup;
        }
        public void Receivingeffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem))
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
            else return;
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

        //Debugging Events.
        /// <summary>
        /// The debugging event for dropping a <see cref="Item"/>
        /// </summary>
        public void Ondrop(DroppingItemEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is dropping {CustomItem.CustomItem.Name}");
            }
            else
                LogManager.Silent($"{ev.Player.Nickname} is dropping {ev.Item}");
        }
        /// <summary>
        /// The debugging event for adding a <see cref="Item"/>
        /// </summary>
        public void Onpickup(ItemAddedEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is adding {CustomItem.CustomItem.Name}");
            }
            else
                LogManager.Silent($"{ev.Player.Nickname} is adding {ev.Item}");
        }
        /// <summary>
        /// The debugging event for using a <see cref="Item"/>
        /// </summary>
        public void Onuse(UsingItemEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is using {CustomItem.CustomItem.Name}");
            }
            else
                LogManager.Silent($"{ev.Player.Nickname} is using {ev.Item}");
        }
        /// <summary>
        /// The debugging event for reloading a <see cref="Firearm"/>
        /// </summary>
        public void Onreloading(ReloadingWeaponEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is reloading {CustomItem.CustomItem.Name}");
            }
            else
                LogManager.Silent($"{ev.Player.Nickname} is reloading {ev.Item}");
        }
        /// <summary>
        /// The debugging event for shooting a <see cref="Firearm"/>
        /// </summary>
        /// <param name="ev"></param>
        public void Onshooting(ShootingEventArgs ev)
        {
            if (Utilities.TryGetSummonedCustomItem(ev.Item.Serial, out SummonedCustomItem CustomItem))
            {
                if (ev.Item.Serial == CustomItem.Serial)
                    LogManager.Silent($"{ev.Player.Nickname} is shooting {CustomItem.CustomItem.Name}");
            }
            else
                LogManager.Silent($"{ev.Player.Nickname} is shooting {ev.Item}");
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
        /// <summary>
        /// Displays the Beta warning if the <see cref="Plugin.Version"/> is a pre release.
        /// </summary>
        public async void OnWaitingForPlayers()
        {
            await Task.Delay(3200);
            Plugin Plugin = new();
            
            LogManager.Warn("===========================================");
            LogManager.Warn($"!WARNING! This is Beta Version {Plugin.Version} for Exiled {Plugin.RequiredExiledVersion} !WARNING!");
            LogManager.Warn("Bugs are to be expected; please report them in our Discord!");
            LogManager.Warn(">> https://discord.gg/5StRGu8EJV <<");
            LogManager.Warn("===========================================");
            LogManager.Warn("Debug logs will be activated due to this!");
            Plugin.Instance.Config.Debug = true;
            Log.DebugEnabled.Add(Plugin.Instance.Assembly);
        }
    }
}