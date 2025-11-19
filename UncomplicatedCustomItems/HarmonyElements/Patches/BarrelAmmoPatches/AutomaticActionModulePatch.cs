using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using Mirror;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AutomaticActionModule))]
    internal static class AutomaticActionModulePatch
    {
        [HarmonyPatch(nameof(AutomaticActionModule.ServerProcessCmd))]
        [HarmonyPrefix]
        public static void ServerProcessCmdPrefix(AutomaticActionModule __instance, NetworkReader reader)
        {
            int startPos = reader.Position;
            byte messageHeader = reader.ReadByte();
            reader.Position = startPos;

            LogManager.Debug($"[ServerProcessCmd] Firearm: {__instance.Firearm?.ItemTypeId}, MessageHeader: {(AutomaticActionModule.MessageHeader)messageHeader}, Owner: {Player.Get(__instance.Firearm.Owner)?.DisplayName}");
        }

        [HarmonyPatch(nameof(AutomaticActionModule.ServerShoot))]
        [HarmonyPrefix]
        public static void ServerShootPrefix(AutomaticActionModule __instance, ReferenceHub primaryTarget)
        {
            LogManager.Debug($"[ServerShoot] Target: {Player.Get(primaryTarget)?.DisplayName}, AmmoStored: {__instance.AmmoStored}, OpenBolt: {__instance.OpenBolt}");
        }

        [HarmonyPatch(nameof(AutomaticActionModule.ServerSendRejection))]
        [HarmonyPrefix]
        public static void ServerSendRejectionPrefix(AutomaticActionModule __instance, AutomaticActionModule.RejectionReason reason, byte errorCode)
        {
            LogManager.Debug($"[REJECTION] Reason: {reason}, ErrorCode: {errorCode}, Firearm: {__instance.Firearm?.ItemTypeId}");
        }

        [HarmonyPatch(nameof(AutomaticActionModule.OnTriggerHeld))]
        [HarmonyPrefix]
        public static void OnTriggerHeldPrefix(AutomaticActionModule __instance)
        {
            LogManager.Debug($"[OnTriggerHeld] Cocked: {__instance._clientCocked.Value}, RateLimiter Ready: {__instance._clientRateLimiter.Ready}");
        }

        [HarmonyPatch(nameof(AutomaticActionModule.ProcessClientShots))]
        [HarmonyPrefix]
        public static void ProcessClientShotsPrefix(AutomaticActionModule __instance)
        {
            LogManager.Debug($"[ProcessClientShots] QueuedShots: {__instance._clientQueuedShots.Count}, Cocked: {__instance._clientCocked.Value}, BoltLocked: {__instance._clientBoltLock.Value}, AmmoChambered: {__instance._clientChambered.Value}");
        }

        [HarmonyPatch("get_ChamberSize")]
        [HarmonyPrefix]
        public static bool Prefix(AutomaticActionModule __instance, ref int __result)
        {
            if (SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem) && summonItem.CustomItem is CustomWeapon customWeapon)
            {
                __result = customWeapon.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }

            if (Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) && customItem.CustomItem.CustomData is WeaponData data)
            {
                __result = data.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(AutomaticActionModule.UpdateServer))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateServerTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new(instructions);
            for (int i = 0; i < codes.Count - 10; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo { Name: "get_Modules" })
                {
                    Label skipLabel = generator.DefineLabel();

                    // Insert the check before the loop
                    List<CodeInstruction> newInstructions = [
                        new CodeInstruction(OpCodes.Ldarg_0), // Load this (AutomaticActionModule instance)
                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ModuleBase), nameof(ModuleBase.Firearm))), // Get Firearm
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(API.Extensions.ItemExtensions), nameof(API.Extensions.ItemExtensions.IsSummonedCustomItem), [typeof(ItemBase)])), // Call IsSummonedCustomItem
                        new CodeInstruction(OpCodes.Brtrue, skipLabel) // If true, skip the busy check
                    ];
                    
                    codes.InsertRange(i, newInstructions);

                    // Find the end of the busy module check - look for the AmmoStored/OpenBolt check
                    for (int j = i + 20; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Callvirt && codes[j].operand is MethodInfo { Name: "get_AmmoStored" or "get_OpenBolt" })
                        {
                            codes[j].labels.Add(skipLabel);
                            break;
                        }
                    }

                    break;
                }
            }

            string text = string.Empty;
            foreach (CodeInstruction code in codes)
                text += $" \n {code}";

            LogManager.Debug($"Successfully patched UpdateServer with ILCode: {text}");
            return codes;
        }
    }
}