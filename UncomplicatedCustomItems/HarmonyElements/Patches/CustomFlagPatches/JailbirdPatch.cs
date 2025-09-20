using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Jailbird;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(JailbirdItem), "ServerProcessCmd")]
    internal static class JailbirdItemChargePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new(instructions);
            
            try
            {
                for (int i = 0; i < codes.Count - 10; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldc_I4_3 && codes[i + 2].opcode == OpCodes.Sub && codes[i + 3].opcode == OpCodes.Switch)
                    {
                        Label skipLabel = generator.DefineLabel();
                        List<CodeInstruction> newInstructions =
                        [
                            // Load JailbirdItem instance
                            new CodeInstruction(OpCodes.Ldarg_0),
                            // Load messageType
                            new CodeInstruction(OpCodes.Ldloc_S, 4),
                            // Call the check method
                            new CodeInstruction(OpCodes.Call, typeof(JailbirdItemChargePatch).GetMethod(nameof(CheckNoCharge), BindingFlags.NonPublic | BindingFlags.Static)),
                            // If false continue to switch
                            new CodeInstruction(OpCodes.Brfalse_S, skipLabel),
                            // If true return from method
                            new CodeInstruction(OpCodes.Ret)
                        ];

                        codes[i].labels.Add(skipLabel);
                        codes.InsertRange(i, newInstructions);
                        
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(JailbirdItemChargePatch)} Transpiler: {ex.Message}\n{ex.StackTrace}");
            }
            
            return codes;
        }
        
        private static bool CheckNoCharge(JailbirdItem instance, JailbirdMessageType messageType)
        {
            try
            {
                if (messageType == JailbirdMessageType.ChargeLoadTriggered || messageType == JailbirdMessageType.ChargeStarted)
                {
                    if (Utilities.TryGetSummonedCustomItem(instance.ItemSerial, out var customItem))
                    {
                        if (customItem.Item.Type == ItemType.Jailbird && customItem.HasModule(CustomFlags.NoCharge))
                        {
                            instance.SendRpc(JailbirdMessageType.ChargeFailed);
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(JailbirdItemChargePatch)}.{nameof(CheckNoCharge)}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}