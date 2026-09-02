using Achievements;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Usables;
using LabApi.Features.Wrappers;
using MEC;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public static class ConsumablePatch
    {
        [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
        public static bool Prefix(Consumable __instance)
        {
            if (SummonedCustomItem.TryGet(__instance.ItemSerial, out SummonedCustomItem? item) && item != null && (item.CustomItem.CustomItemType is CustomItemType.Medikit or CustomItemType.Adrenaline or CustomItemType.Painkillers || (item.CustomItem.CustomItemType is CustomItemType.SCPItem && item.CustomItem.Item is ItemType.SCP207 or ItemType.AntiSCP207 or ItemType.SCP500 or ItemType.SCP1853)))
            {
                Apply(__instance, item.CustomItem);
                __instance._alreadyActivated = true;
                return false;
            }

            if (APICustomItem.TryGet(__instance.ItemSerial, out APICustomItem? apiitem) && apiitem != null && apiitem is CustomMedkit or CustomPainkillers or CustomAdrenaline or CustomSCP207 or CustomSCP1853)
            {
                Apply(__instance, apiitem);
                __instance._alreadyActivated = true;
                return false;
            }

            return true;
        }

        private static void Apply(Consumable instance, CustomItem item)
        {
            switch (item.CustomData)
            {
                case MedikitData medkit:
                    instance.Owner.playerStats.GetModule<HealthStat>().ServerHeal(medkit.Health);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    break;

                case PainkillersData painkillers:
                    Timing.RunCoroutine(PainkillersCoroutine(instance, painkillers));
                    break;

                case AdrenalineData adrenaline:
                    instance.Owner.playerStats.GetModule<StaminaStat>().AddAmount(adrenaline.StaminaGain);
                    instance.Owner.playerStats.GetModule<AhpStat>().ServerAddProcess(adrenaline.Amount);
                    instance.Owner.playerEffectsController.EnableEffect<Invigorated>(8f, addDuration: true);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    instance.Owner.playerEffectsController.DisableEffect<AmnesiaVision>();
                    break;

                case SCP207Data scp207:
                    instance.Owner.playerStats.GetModule<StaminaStat>().CurValue = scp207.StaminaGain;
                    instance.Owner.playerStats.GetModule<HealthStat>().ServerHeal(scp207.InstantHealth);
                    if (scp207.Apply207Effect && instance.Owner.playerEffectsController.TryGetEffect<CustomPlayerEffects.Scp207>(out var playerEffect))
                    {
                        byte intensity = playerEffect.Intensity;
                        if (scp207.BypassMax)
                        {
                            instance.Owner.playerEffectsController.ChangeState<CustomPlayerEffects.Scp207>(++intensity);
                        }
                        else if (intensity < 4)
                        {
                            instance.Owner.playerEffectsController.ChangeState<CustomPlayerEffects.Scp207>(++intensity);
                        }
                    }

                    if (!scp207.RemoveItemAfterUse)
                        Timing.CallDelayed(0.5f, () => new SummonedCustomItem(item, Player.Get(instance.Owner)));

                    break;

                case SCP500Data scp500 when instance is Scp500 scp:
                    if (!scp500.ApplyHeal)
                        break;

                    HealthStat module = instance.Owner.playerStats.GetModule<HealthStat>();
                    if (module.CurValue < 20f && scp500.AllowAchievement)
                        AchievementHandlerBase.ServerAchieve(instance.Owner.networkIdentity.connectionToClient, AchievementName.CrisisAverted);

                    module.ServerHeal(scp500.InstantHealth);
                    instance.ServerAddRegeneration(scp._healProgress, scp500.RegenSpeedMultiplier, scp500.HpGainMultiplier);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    break;

                case SCP1853Data scp1853:
                    if (scp1853.Apply1853Effect)
                        instance.Owner.playerEffectsController.EnableEffect<Scp1853>();

                    if (!scp1853.RemoveItemAfterUse)
                        Timing.CallDelayed(0.5f, () => new SummonedCustomItem(item, Player.Get(instance.Owner)));
                    break;
            }
        }

        private static void Apply(Consumable instance, APICustomItem item)
        {
            switch (item)
            {
                case CustomMedkit medkit:
                    instance.Owner.playerStats.GetModule<HealthStat>().ServerHeal(medkit.Health);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    break;

                case CustomPainkillers painkillers:
                    Timing.RunCoroutine(PainkillersCoroutine(instance, painkillers));
                    break;

                case CustomAdrenaline adrenaline:
                    instance.Owner.playerStats.GetModule<StaminaStat>().AddAmount(adrenaline.StaminaGain);
                    instance.Owner.playerStats.GetModule<AhpStat>().ServerAddProcess(adrenaline.Amount);
                    instance.Owner.playerEffectsController.EnableEffect<Invigorated>(8f, addDuration: true);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    instance.Owner.playerEffectsController.DisableEffect<AmnesiaVision>();
                    break;

                case CustomSCP207 custom207:
                    instance.Owner.playerStats.GetModule<StaminaStat>().CurValue = custom207.StaminaGain;
                    instance.Owner.playerStats.GetModule<HealthStat>().ServerHeal(custom207.InstantHealth);
                    if (custom207.Apply207Effect && instance.Owner.playerEffectsController.TryGetEffect<CustomPlayerEffects.Scp207>(out var playerEffect))
                    {
                        byte intensity = playerEffect.Intensity;
                        if (custom207.BypassMax)
                        {
                            instance.Owner.playerEffectsController.ChangeState<CustomPlayerEffects.Scp207>(++intensity);
                        }
                        else if (intensity < 4)
                        {
                            instance.Owner.playerEffectsController.ChangeState<CustomPlayerEffects.Scp207>(++intensity);
                        }
                    }

                    if (!custom207.RemoveItemAfterUse)
                        Timing.CallDelayed(0.5f, () => new SummonedAPICustomItem(item, Player.Get(instance.Owner)));
                    break;

                case CustomSCP1853 custom1853:
                    if (custom1853.Apply1853Effect)
                        instance.Owner.playerEffectsController.EnableEffect<Scp1853>();

                    if (!custom1853.RemoveItemAfterUse)
                        Timing.CallDelayed(0.5f, () => new SummonedAPICustomItem(item, Player.Get(instance.Owner)));
                    break;
            }
        }

        internal static IEnumerator<float> PainkillersCoroutine(Consumable consumable, PainkillersData data)
        {
            float TotalHealed = 0;
            Player player = Player.Get(consumable.Owner);
            yield return Timing.WaitForSeconds(data.TimeBeforeStartHealing);

            while (TotalHealed < data.TotalHealing && player.IsAlive)
            {
                float healAmount = Math.Min(data.TickHeal, data.TotalHealing - TotalHealed);
                player.Heal(healAmount);
                TotalHealed += healAmount;
                yield return Timing.WaitForSeconds(data.TickTime);
            }
        }

        internal static IEnumerator<float> PainkillersCoroutine(Consumable consumable, CustomPainkillers data)
        {
            float TotalHealed = 0;
            Player player = Player.Get(consumable.Owner);
            yield return Timing.WaitForSeconds(data.TimeBeforeStartHealing);

            while (TotalHealed < data.TotalHealing && player.IsAlive)
            {
                float healAmount = Math.Min(data.TickHeal, data.TotalHealing - TotalHealed);
                player.Heal(healAmount);
                TotalHealed += healAmount;
                yield return Timing.WaitForSeconds(data.TickTime);
            }
        }
    }
}