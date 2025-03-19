using HarmonyLib;
using InventorySystem.Items.Usables;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
    internal class ConsumablePatch
    {
        static bool Prefix(Consumable __instance)
        {
            if (SummonedCustomItem.TryGet(__instance.ItemSerial, out SummonedCustomItem summonedCustomItem))
                return !summonedCustomItem.HandleCustomAction(__instance);

            return true;
        }
    }
}