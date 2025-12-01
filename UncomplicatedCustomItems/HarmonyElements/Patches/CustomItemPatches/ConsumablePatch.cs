using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Usables;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
    internal class ConsumablePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Consumable __instance)
        {
            if (API.Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out SummonedCustomItem CustomItem))
            {
                LogManager.Debug($"Checking if {CustomItem.CustomItem.Name} is Adrenaline, Painkillers or Medkit");
                if (CustomItem.CustomItem.CustomItemType is CustomItemType.Adrenaline or CustomItemType.Painkillers or CustomItemType.Medikit)
                {
                    LogManager.Debug($"{CustomItem.CustomItem.Name} is Adrenaline, Painkillers or Medkit\n Applying patch...");
                    return false;
                }
            }

            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var item))
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
        /// <param name="Data"></param>
        /// <returns></returns>
        internal static IEnumerator<float> PainkillersCoroutine(Player player, CustomPainkillers Data)
        {
            float TotalHealed = 0;
            yield return Timing.WaitForSeconds(Data.TimeBeforeStartHealing);
            while (TotalHealed < Data.TotalHealing && player.IsAlive)
            {
                player.Heal(Data.TickHeal);
                TotalHealed += Data.TickHeal;
                yield return Timing.WaitForSeconds(Data.TickTime);
            }
        }
    }
}