using HarmonyLib;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(InventorySystem.Items.Usables.Painkillers), nameof(InventorySystem.Items.Usables.Painkillers.OnEffectsActivated))]
    internal class PainkillersPatch
    {
        static bool Prefix()
        {
            return false;
        }
    }
}