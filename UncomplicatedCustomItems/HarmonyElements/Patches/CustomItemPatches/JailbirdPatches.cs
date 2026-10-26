using HarmonyLib;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Jailbird;
using LabApi.Features.Wrappers;
using Mirror;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Events.Arguments.JailbirdEvents;
using UncomplicatedCustomItems.Events.Handlers;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    [HarmonyPatch(typeof(JailbirdDeteriorationTracker))]
    internal static class JailbirdPatches
    {
        [HarmonyPatch(nameof(JailbirdDeteriorationTracker.RecheckUsage))]
        [HarmonyPrefix]
        public static bool RecheckUsagePrefix(JailbirdDeteriorationTracker __instance, out JailbirdWearState __state)
        {
            __state = __instance.WearState;

            if (__instance._jailbird == null)
                return true;

            if (TryGetLockedWearState(__instance._jailbird.ItemSerial, out JailbirdWearState locked))
            {
                __instance._jailbird.TotalMeleeDamageDealt = 0f;
                __instance._jailbird.TotalChargesPerformed = 0;

                JailbirdDeteriorationTracker.ReceivedStates[__instance._jailbird.ItemSerial] = locked;

                if (__state != locked)
                {
                    using AutosyncRpc sync = new(__instance._jailbird.ItemId, out NetworkWriter writer);
                    writer.WriteByte((byte)JailbirdMessageType.UpdateState);
                    writer.WriteByte((byte)locked);
                    sync.Send();
                }

                return false;
            }

            JailbirdWearState damageState = __instance.StateForTotalDamage(__instance._jailbird.TotalMeleeDamageDealt);
            JailbirdWearState chargesState = __instance.StateForCharges(__instance._jailbird.TotalChargesPerformed);
            JailbirdWearState newState = (damageState > chargesState) ? damageState : chargesState;

            if (__state != newState)
            {
                ChangingWearStateEventArgs args = new(LabApi.Features.Wrappers.JailbirdItem.Get(__instance._jailbird), newState, __state, Player.Get(__instance._jailbird.Owner));
                JailbirdEvents.OnWearStateChanging(args);

                if (!args.IsAllowed)
                {
                    JailbirdDeteriorationTracker.ReceivedStates[__instance._jailbird.ItemSerial] = __state;
                    AutosyncRpc sync = new(__instance._jailbird.ItemId, out NetworkWriter writer);
                    writer.WriteByte(0);
                    writer.WriteByte((byte)__state);
                    sync.Send();
                    sync.Dispose();

                    return false;
                }

                if (args.NewWearState != newState)
                {
                    JailbirdDeteriorationTracker.ReceivedStates[__instance._jailbird.ItemSerial] = args.NewWearState;
                    AutosyncRpc sync = new(__instance._jailbird.ItemId, out NetworkWriter writer);
                    writer.WriteByte(0);
                    writer.WriteByte((byte)args.NewWearState);
                    sync.Send();
                    sync.Dispose();

                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(nameof(JailbirdDeteriorationTracker.RecheckUsage))]
        [HarmonyPostfix]
        public static void RecheckUsagePostfix(JailbirdDeteriorationTracker __instance, JailbirdWearState __state)
        {
            if (__state != __instance.WearState)
                JailbirdEvents.OnWearStateChanged(new(LabApi.Features.Wrappers.JailbirdItem.Get(__instance._jailbird), __instance.WearState, __state, Player.Get(__instance._jailbird.Owner)));
        }

        private static bool TryGetLockedWearState(ushort serial, out JailbirdWearState locked)
        {
            locked = default;

            if (Utilities.TryGetSummonedCustomItem(serial, out SummonedCustomItem? summoned) && summoned?.CustomItem?.CustomItemType is CustomItemType.Jailbird && summoned.CustomItem.CustomData is JailbirdData data && !data.AllowWearStateChanges)
            {
                locked = data.WearState;
                return true;
            }

            if (SummonedAPICustomItem.TryGet(serial, out SummonedAPICustomItem? api) && api?.CustomItem is CustomJailbird jailbird && jailbird.LockWearState)
            {
                locked = jailbird.WearState;
                return true;
            }

            return false;
        }
    }
}
