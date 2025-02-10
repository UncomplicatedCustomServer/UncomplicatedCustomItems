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


namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {

        private Dictionary<Pickup, Exiled.API.Features.Toys.Light> ActiveLights = new Dictionary<Pickup, Exiled.API.Features.Toys.Light>();
        public float Amount { get; set; } = 8f;
        public float Percentage => 0.5f;
        public static Assembly EventHandlerAssembly => Loader.Plugins.Where(plugin => plugin.Name is "Exiled.Events").FirstOrDefault()?.Assembly;

        public static Type PlayerHandler => EventHandlerAssembly?.GetTypes().Where(x => x.FullName == "Exiled.Events.Handlers.Player").FirstOrDefault();
        public void OnHurt(HurtEventArgs ev)
        {
            LogManager.Debug("OnHurt event is being triggered");
            if (ev.Player is not null && ev.Attacker is not null && ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem summonedCustomItem))
            {
                 LogManager.Debug("Fuck all event is being triggered");
                summonedCustomItem.LastDamageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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
                    LogManager.Warn("ERROR: InfiniteAmmo flag was triggered but no valid firearm found.");
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
                    LogManager.Warn("ERROR: DieOnUse flag was triggered but couldnt be ran.");
                }
            }
        }
        public void OnItemUse(UsedItemEventArgs ev)
        {

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<DieOnUse>())
            {
                if (ev.Item  != null)
                {
                    ev.Player.Kill(DamageType.Custom);
                    LogManager.Debug("DieOnUse triggered: user killed.");
                }
                else
                {
                    LogManager.Warn("ERROR: DieOnUse flag was triggered but couldnt be ran.");
                }
            }
        }
        public void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
        {

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<WorkstationBan>())
            {
                if (ev.Player  != null)
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint, Plugin.Instance.Config.WorkstationBanHintDuration);
                }
                else
                {
                    LogManager.Warn("ERROR: WorkstationBan flag was triggered but couldnt be ran.");
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
                    LogManager.Warn("ERROR: WorkstationBan flag was triggered but couldnt be ran.");
                }
            }
        }
        public void OnDrop(DroppedItemEventArgs ev)
        {

            if (ev.Pickup != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<ItemGlow>())
            {
                if (ev.Pickup != null)
                {
                    SpawnLightOnItem(ev.Pickup);
                }
                else
                {
                    LogManager.Warn("ERROR: ItemGlow flag was triggered but couldnt be ran.");
                }
            }
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
                    LogManager.Warn("ERROR: ItemGlow flag was triggered but couldnt be ran.");
                }
            }
        }
        public void DestroyLightOnItem(Pickup pickup)
        {
            LogManager.Debug("DestroyLightOnItem method triggered");
            if (pickup == null || !ActiveLights.ContainsKey(pickup))
                return;

            Exiled.API.Features.Toys.Light ItemLight = ActiveLights[pickup];
            if (ItemLight != null && ItemLight.Base != null)
            {
                GameObject.Destroy(ItemLight.Base.gameObject);
            }

            ActiveLights.Remove(pickup);
            LogManager.Debug("Light successfully destroyed.");
        }
        public static Color GetColorFromConfig(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "red": return Color.red;
                case "green": return Color.green;
                case "blue": return Color.blue;
                case "yellow": return Color.yellow;
                case "cyan": return Color.cyan;
                case "magenta": return Color.magenta;
                case "white": return Color.white;
                case "black": return Color.black;
                case "gray": return Color.gray;
                case "grey": return Color.gray;
                default:
                    LogManager.Warn($"Invalid color '{colorName}' in config. Using default blue.");
                    return Color.blue;
            }
        }
        public void SpawnLightOnItem(Pickup pickup)
        {
            LogManager.Debug("SpawnLightOnItem method triggered");
            if (pickup == null || pickup.Base == null || pickup.Base.gameObject == null)
                return;

            GameObject ItemGameObject = pickup.Base.gameObject;
            
            string colorString = Plugin.Instance.Config.GlowColor;
            Color lightColor = GetColorFromConfig(colorString);

            var Light = Exiled.API.Features.Toys.Light.Create(pickup.Position);
                
            Light.Color = lightColor;
            Light.Intensity = 0.7f;
            Light.Range = 0.5f;
            Light.ShadowType = LightShadows.None;

            Light.Base.gameObject.transform.SetParent(ItemGameObject.transform, worldPositionStays: true);
            LogManager.Debug($"Item Light should be spawned at position: {Light.Base.transform.position}");
            ActiveLights[pickup] = Light;
        }

        public async void OnWaitingForPlayers()
        {
            await Task.Delay(3200);

            LogManager.Info("===========================================");
            LogManager.Info("!WARNING! This is Beta Version 3.0.0 !WARNING!");
            LogManager.Info("Bugs are to be expected; please report them in our Discord");
            LogManager.Info(">> https://discord.gg/5StRGu8EJV <<");
            LogManager.Info("===========================================");
        }
    }
}
