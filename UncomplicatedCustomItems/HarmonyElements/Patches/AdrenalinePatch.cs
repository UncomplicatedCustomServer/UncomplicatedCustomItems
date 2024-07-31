using HarmonyLib;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(InventorySystem.Items.Usables.Adrenaline), nameof(InventorySystem.Items.Usables.Adrenaline.OnEffectsActivated))]
    internal class AdrenalinePatch
    {
        static bool Prefix()
        {
            return false;
        }
    }
}
