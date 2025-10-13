using System.Collections.Generic;
using HarmonyLib;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    internal static class UsableItemPatch
    {
        [HarmonyPatch(typeof(InventorySystem.Items.Usables.Consumable), "ActivateEffects")]
        public static bool Prefix(InventorySystem.Items.Usables.Consumable __instance)
        {
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