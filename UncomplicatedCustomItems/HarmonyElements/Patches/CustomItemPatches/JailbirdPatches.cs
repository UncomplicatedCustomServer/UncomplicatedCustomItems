using HarmonyLib;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Jailbird;
using LabApi.Features.Wrappers;
using Mirror;
using UncomplicatedCustomItems.Events.Arguments.JailbirdEvents;
using UncomplicatedCustomItems.Events.Handlers;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    [HarmonyPatch(typeof(JailbirdDeteriorationTracker))]
    internal static class JailbirdPatches
    {
        // Broke all Jailbirds :|
        /*
        [HarmonyPatch(nameof(JailbirdDeteriorationTracker.Setup))]
        [HarmonyPrefix]
        public static void SetupPrefix(JailbirdDeteriorationTracker __instance)
        {
            if (Utilities.TryGetSummonedCustomItem(__instance._jailbird.ItemSerial, out var item) && item.CustomItem.CustomItemType == CustomItemType.Jailbird && item.CustomItem.CustomData is JailbirdData data)
            {
                __instance._chargesToWearState = data.ChargesToWearState;
                __instance._damageToWearState = data.DamageToWearState;
            }
        }
        */

        private static JailbirdWearState previousState;

        [HarmonyPatch(nameof(JailbirdDeteriorationTracker.RecheckUsage))]
        [HarmonyPrefix]
        public static bool RecheckUsagePrefix(JailbirdDeteriorationTracker __instance)
        {
            previousState = __instance.WearState;

            JailbirdWearState damageState = __instance.StateForTotalDamage(__instance._hitreg.TotalMeleeDamageDealt);
            JailbirdWearState chargesState = __instance.StateForCharges(__instance._jailbird.TotalChargesPerformed);
            JailbirdWearState newState = (damageState > chargesState) ? damageState : chargesState;

            if (previousState != newState)
            {
                ChangingWearStateEventArgs args = new(LabApi.Features.Wrappers.JailbirdItem.Get(__instance._jailbird), newState, previousState, Player.Get(__instance._jailbird.Owner));
                JailbirdEvents.OnWearStateChanging(args);

                if (!args.IsAllowed)
                {
                    JailbirdDeteriorationTracker.ReceivedStates[__instance._jailbird.ItemSerial] = previousState;
                    using (new AutosyncRpc(__instance._jailbird.ItemId, out NetworkWriter writer))
                    {
                        writer.WriteByte(0);
                        writer.WriteByte((byte)previousState);
                    }

                    return false;
                }

                if (args.NewWearState != newState)
                {
                    JailbirdDeteriorationTracker.ReceivedStates[__instance._jailbird.ItemSerial] = args.NewWearState;
                    using (new AutosyncRpc(__instance._jailbird.ItemId, out NetworkWriter writer))
                    {
                        writer.WriteByte(0);
                        writer.WriteByte((byte)args.NewWearState);
                    }

                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(nameof(JailbirdDeteriorationTracker.RecheckUsage))]
        [HarmonyPostfix]
        public static void RecheckUsagePostfix(JailbirdDeteriorationTracker __instance)
        {
            if (previousState != __instance.WearState)
                JailbirdEvents.OnWearStateChanged(new(LabApi.Features.Wrappers.JailbirdItem.Get(__instance._jailbird), __instance.WearState, previousState, Player.Get(__instance._jailbird.Owner)));
        }
    }
}
