using HarmonyLib;
using InventorySystem.Items.Usables;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
    internal class ConsumablePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Consumable __instance)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out SummonedCustomItem CustomItem))
            {
                LogManager.Debug($"Checking if {CustomItem.CustomItem.Name} is Adrenaline, Painkillers or Medkit");
                if (CustomItem.CustomItem.CustomItemType == CustomItemType.Adrenaline || CustomItem.CustomItem.CustomItemType == CustomItemType.Painkillers || CustomItem.CustomItem.CustomItemType == CustomItemType.Medikit)
                {
                    LogManager.Debug($"{CustomItem.CustomItem.Name} is Adrenaline, Painkillers or Medkit\n Applying patch...");
                    return false;
                }
                else
                {
                    LogManager.Debug($"{CustomItem.CustomItem.Name} is not Adrenaline, Painkillers or Medkit\n Aborting patch...");
                    return true;
                }
            }
            else
            {
                LogManager.Debug($"{__instance.Name} Is not a UCI CustomItem!\n Aborting patch...");
                return true;
            }
        }
    }
}