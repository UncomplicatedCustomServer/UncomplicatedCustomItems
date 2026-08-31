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
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public static class ConsumablePatch
    {
        [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
        public static bool Prefix(Consumable __instance)
        {
            if (SummonedCustomItem.TryGet(__instance.ItemSerial, out SummonedCustomItem? item) && item != null && item.CustomItem.CustomItemType is CustomItemType.Medikit or CustomItemType.Adrenaline or CustomItemType.Painkillers)
            {
                Apply(__instance, item.CustomItem.CustomData);
                __instance._alreadyActivated = true;
                return false;
            }

            if (APICustomItem.TryGet(__instance.ItemSerial, out APICustomItem? apiitem) && apiitem != null && apiitem is CustomMedkit or CustomPainkillers or CustomAdrenaline)
            {
                Apply(__instance, apiitem);
                __instance._alreadyActivated = true;
                return false;
            }

            return true;
        }

        private static void Apply(Consumable instance, IData data)
        {
            switch (data)
            {
                case IMedikitData medkit:
                    instance.Owner.playerStats.GetModule<HealthStat>().ServerHeal(medkit.Health);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    break;

                case IPainkillersData painkillers:
                    Timing.RunCoroutine(PainkillersCoroutine(instance, painkillers));
                    break;

                case IAdrenalineData adrenaline:
                    instance.Owner.playerStats.GetModule<StaminaStat>().AddAmount(adrenaline.StaminaGain);
                    instance.Owner.playerStats.GetModule<AhpStat>().ServerAddProcess(adrenaline.Amount);
                    instance.Owner.playerEffectsController.EnableEffect<Invigorated>(8f, addDuration: true);
                    instance.Owner.playerEffectsController.UseMedicalItem(instance);
                    instance.Owner.playerEffectsController.DisableEffect<AmnesiaVision>();
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
            }
        }

        internal static IEnumerator<float> PainkillersCoroutine(Consumable consumable, IPainkillersData data)
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