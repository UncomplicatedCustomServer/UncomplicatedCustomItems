using System;
using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Usables;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
    internal class ConsumablePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Consumable __instance)
        {
            if (API.Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out SummonedCustomItem? CustomItem) && CustomItem != null)
            {
                LogManager.Debug($"Checking if {CustomItem.CustomItem.Name} is Adrenaline, Painkillers or Medkit");
                if (CustomItem.CustomItem.CustomItemType is CustomItemType.Adrenaline or CustomItemType.Painkillers or CustomItemType.Medikit)
                {
                    LogManager.Debug($"{CustomItem.CustomItem.Name} is Adrenaline, Painkillers or Medkit\n Applying patch...");
                    return false;
                }
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var item) && item != null)
            {
                switch (item.CustomItem)
                {
                    case CustomMedkit medkit:
                        Player.Get(__instance.Owner).Heal(medkit.Health);
                        return false;

                    case CustomPainkillers painkillers:
                        Timing.RunCoroutine(PainkillersCoroutine(Player.Get(__instance.Owner), painkillers));
                        return false;

                    case CustomAdrenaline adrenaline:
                        Player.Get(__instance.Owner).CreateAhpProcess(adrenaline.Amount, limit: 1000f, decay: adrenaline.Decay, efficacy: adrenaline.Efficacy, sustain: adrenaline.Sustain, adrenaline.Persistant);
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reproduce the SCP:SL <see cref="ItemType.Painkillers"/> healing process but with custom things :)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static IEnumerator<float> PainkillersCoroutine(Player player, CustomPainkillers data)
        {
            float TotalHealed = 0;
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