using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Loader;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomModules;
using UncomplicatedCustomItems.Extensions;

namespace UncomplicatedCustomItems.Events
{
    internal class EventHandler
    {
        public float Amount { get; set; } = 8f;
        public float Percentage => 0.5f;
        public static Assembly EventHandlerAssembly => Loader.Plugins.Where(plugin => plugin.Name is "Exiled.Events").FirstOrDefault()?.Assembly;

        public static Type PlayerHandler => EventHandlerAssembly?.GetTypes().Where(x => x.FullName == "Exiled.Events.Handlers.Player").FirstOrDefault();
        public void OnHurt(HurtEventArgs ev)
        {
            Log.Debug("OnHurt event is being triggered");
            if (ev.Player is not null && ev.Attacker is not null && ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem summonedCustomItem))
            {
                 Log.Debug("Fuck all event is being triggered");
                summonedCustomItem.LastDamageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<LifeSteal>())
                {
                    Log.Debug("LifeSteal custom flag is being triggered");

                    if (Amount > 0)
                    {
                        ev.Attacker.Heal(Amount);
                        Log.Debug($"LifeSteal custom flag triggered, healed {Amount} HP");
                    }
                }

                if (ev.Attacker.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<HalfLifeSteal>())
                {
                    Log.Debug("HalfLifeSteal custom flag is being triggered");

                    if (Amount > 0)
                    {
                        float HealedAmount = Amount * Percentage;
                        ev.Attacker.Heal(HealedAmount);
                        Log.Debug($"HalfLifeSteal custom flag triggered, healed {HealedAmount} HP");
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
                    if (ev.Firearm is Firearm firearm)
                    {
                        firearm.MagazineAmmo = firearm.MaxMagazineAmmo;
                        Log.Debug($"InfiniteAmmo flag was triggered: magazine refilled to {firearm.MagazineAmmo}");
                    }
                }
                else
                {
                    Log.Warn("ERROR: InfiniteAmmo flag was triggered but no valid firearm found.");
                }
            }

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem CustomItem) && CustomItem.HasModule<DieOnUse>())
            {
                if (ev.Item  != null)
                {
                    ev.Player.Kill(DamageType.Unknown);
                    Log.Debug("DieOnUse triggered: player killed.");
                }
                else
                {
                    Log.Warn("ERROR: DieOnUse flag was triggered but couldnt be ran.");
                }
            }
        }
        public void OnItemUse(UsedItemEventArgs ev)
        {

            if (ev.Player != null && ev.Player.TryGetSummonedInstance(out SummonedCustomItem customItem) && customItem.HasModule<DieOnUse>())
            {
                if (ev.Item  != null)
                {
                    ev.Player.Kill(DamageType.Unknown);
                    Log.Debug("DieOnUse triggered: user killed.");
                }
                else
                {
                    Log.Warn("ERROR: DieOnUse flag was triggered but couldnt be ran.");
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
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint, Plugin.Instance.Config.WorkstationBanDuration);
                }
                else
                {
                    Log.Warn("ERROR: WorkstationBan flag was triggered but couldnt be ran.");
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
                    ev.Player.ShowHint(Plugin.Instance.Config.WorkstationBanHint, Plugin.Instance.Config.WorkstationBanDuration);
                }
                else
                {
                    Log.Warn("ERROR: WorkstationBan flag was triggered but couldnt be ran.");
                }
            }
        }
        public async void OnWaitingForPlayers()
        {
            await Task.Delay(3200);

            Log.Info("===========================================");
            Log.Info("!WARNING! This is Beta Version 3.0.0 !WARNING!");
            Log.Info("Bugs are to be expected; please report them in our Discord");
            Log.Info(">> https://discord.gg/5StRGu8EJV <<");
            Log.Info("===========================================");
        }
    }
}
