using HarmonyLib;
using InventorySystem.Items.Usables.Scp1344;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(Scp1344Item), nameof(Scp1344Item.ActivateFinalEffects))]
    public class SCP1344EffectPatch
    {
        public static bool Prefix(Scp1344Item __instance)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.ItemSerial, out var summonedCustomItem) || summonedCustomItem == null)
                return true;

            if (summonedCustomItem.CustomItem.CustomItemType is CustomItemType.SCPItem && summonedCustomItem.CustomItem.Item is ItemType.SCP1344)
            {
                if (summonedCustomItem.CustomItem.CustomData is not SCP1344Data data)
                    return true;
                    
                __instance.Scp1344Effect.IsEnabled = data.Apply1344Effect;
                __instance.BlindnessEffect.IsEnabled = data.ApplyBlindnessEffect;
                return false;
            }

            return true;
        }
    }
}
