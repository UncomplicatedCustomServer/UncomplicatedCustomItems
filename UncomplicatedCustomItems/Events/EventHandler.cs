using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using Exiled.Loader;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomModules;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Extensions;
using UnityEngine;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Light = Exiled.API.Features.Toys.Light;
using UncomplicatedCustomItems.Interfaces;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Exiled.API.Features.Toys;
using VoiceChat.Networking;
using System.IO;
using AdminToys;
using UncomplicatedCustomItems.API;


namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        private Dictionary<Pickup, Light> ActiveLights = [];
        private Dictionary<Player, Light> ActiveHandLights = [];
        public float Amount { get; set; } = 0f;
        public float Percentage = 0.5f;
        public static Assembly EventHandlerAssembly => Loader.Plugins.Where(plugin => plugin.Name is "Exiled.Events").FirstOrDefault()?.Assembly;

        public static Type PlayerHandler => EventHandlerAssembly?.GetTypes().Where(x => x.FullName == "Exiled.Events.Handlers.Player").FirstOrDefault();
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
                if (ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<LifeSteal>())
                {
                    LogManager.Debug("LifeSteal custom flag is being triggered");

                    if (Amount > 0)
                    {
                        ev.Attacker.Heal(Amount);
                        LogManager.Debug($"LifeSteal custom flag triggered, healed {Amount} HP");
                    }
                }

                if (ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<HalfLifeSteal>())
                {
                    LogManager.Debug("HalfLifeSteal custom flag is being triggered");

                    if (Amount > 0)
                    {
                        float HealedAmount = Amount * Percentage;
                        ev.Attacker.Heal(HealedAmount);
                        LogManager.Debug($"HalfLifeSteal custom flag triggered, healed {HealedAmount} HP");
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

        }
        public void OnShooting(ShootingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<InfiniteAmmo>())
            {
                if (ev.Firearm != null)
                {
                    if (ev.Firearm is Firearm firearm2)
                    {
                        firearm2.MagazineAmmo = firearm2.MaxMagazineAmmo;
                        LogManager.Debug($"InfiniteAmmo flag was triggered: magazine refilled to {firearm2.MagazineAmmo}");
                    }
                }
                else
                {
                    LogManager.Error("InfiniteAmmo flag was triggered but no valid firearm found.");
                }
            }
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<DieOnUse>())
            {
                if (ev.Item != null)
                {
                    ev.Player.Kill(DamageType.Custom);
                    LogManager.Debug("DieOnUse triggered: player killed.");
                }
                else
                {
                    LogManager.Error("DieOnUse flag was triggered but couldnt be ran.");
                }
            }
        }
        public void OnItemUse(UsedItemEventArgs ev)
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
                    LogManager.Error("DieOnUse flag was triggered but couldnt be ran.");
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
                    LogManager.Error("WorkstationBan flag was triggered but couldnt be ran.");
                }
            }
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
                    LogManager.Error("WorkstationBan flag was triggered but couldnt be ran.");
                }
            }
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
                                LogManager.Error($"Failed to parse color: {flagSetting.GlowColor}");
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
        public void OnUsingItem(UsingItemEventArgs ev)
        {
            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<EffectWhenUsed>())
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
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for {customItem.CustomItem}");
                                return;
                            }
                            if (flagSetting.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for {customItem.CustomItem}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for {customItem.CustomItem}");
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
                        LogManager.Error("No FlagSettings found on {customItem.CustomItem}");
                    }
                }
                else
                {
                    LogManager.Error("EffectWhenUsed Flag was triggered but couldnt be ran.");
                }
            }
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
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for {customItem.CustomItem}");
                                return;
                            }
                            if (flagSetting.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for {customItem.CustomItem}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for {customItem.CustomItem}");
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
                        LogManager.Error($"No FlagSettings found on {customItem.CustomItem}");
                    }
                }
                else
                {
                    LogManager.Error("EffectWhenUsed Flag was triggered but couldnt be ran.");
                }
            }
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
                                LogManager.Warn($"Invalid Effect: {flagSetting.Effect} for {CustomItem.CustomItem}");
                                return;
                            }
                            if (flagSetting.EffectDuration <= -2)
                            {
                                LogManager.Warn($"Invalid Duration: {flagSetting.EffectDuration} for {CustomItem.CustomItem}");
                                return;
                            }
                            if (flagSetting.EffectIntensity <= 0)
                            {
                                LogManager.Warn($"Invalid intensity: {flagSetting.EffectIntensity} for {CustomItem.CustomItem}");
                                return;
                            }

                            LogManager.Debug($"Applying effect {flagSetting.Effect} at intensity {flagSetting.EffectIntensity}, duration is {flagSetting.EffectDuration} to {ev.Target}");
                            EffectType Effect = flagSetting.Effect;
                            float Duration = flagSetting.EffectDuration;
                            byte Intensity = flagSetting.EffectIntensity;
                            ev.Target?.EnableEffect(Effect, Intensity, Duration, true);
                        }
                    }
                    else
                    {
                        LogManager.Error("No FlagSettings found on {CustomItem.CustomItem}");
                    }
                }
                else
                {
                    LogManager.Error("EffectShot Flag was triggered but couldnt be ran.");
                }
            }
        }

        public void Onroundend(RoundEndedEventArgs ev)
        {
            Exiled.Events.Handlers.Map.PickupDestroyed -= OnPickup;
        }

        public void OnPickup(PickupDestroyedEventArgs ev)
        {
            if (ev.Pickup != null)
            {
                if (ev.Pickup != null)
                {
                    DestroyLightOnItem(ev.Pickup);
                }
                else
                {
                    LogManager.Error("Couldnt destroy light on {Pickup}.");
                }
            }
        }
        public void DestroyLightOnItem(Pickup pickup)
        {
            LogManager.Debug("DestroyLightOnItem method triggered");
            if (pickup == null || !ActiveLights.ContainsKey(pickup))
                return;

            Light ItemLight = ActiveLights[pickup];
            if (ItemLight != null && ItemLight.Base != null)
            {
                GameObject.Destroy(ItemLight.Base.gameObject);
            }

            ActiveLights.Remove(pickup);
            LogManager.Debug("Light successfully destroyed.");
        }
        public async void OnWaitingForPlayers()
        {
            await Task.Delay(3200);

            LogManager.Warn("===========================================");
            LogManager.Warn("!WARNING! This is Beta Version 3.1.0 !WARNING!");
            LogManager.Warn("Bugs are to be expected; please report them in our Discord");
            LogManager.Warn(">> https://discord.gg/5StRGu8EJV <<");
            LogManager.Warn("===========================================");
        }
    }
}