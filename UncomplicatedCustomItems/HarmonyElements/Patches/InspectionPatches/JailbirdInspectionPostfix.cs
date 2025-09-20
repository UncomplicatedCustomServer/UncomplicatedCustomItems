using HarmonyLib;
using JailbirdItem = InventorySystem.Items.Jailbird.JailbirdItem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using InventorySystem.Items.Jailbird;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.InspectionPatches
{
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
    public static class JailbirdInspectionTranspiler
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!found && codes[i].opcode == OpCodes.Stloc_S && codes[i].operand.ToString() == "4" && i > 0 && codes[i - 1].opcode == OpCodes.Ldloc_0)
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0)); // Load this
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_0)); // Load messageType
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, typeof(JailbirdInspectionTranspiler).GetMethod(nameof(HandleInspection), BindingFlags.Static | BindingFlags.NonPublic)));
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Stloc_0)
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0)); // Load this
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_0)); // Load messageType
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, typeof(JailbirdInspectionTranspiler).GetMethod(nameof(HandleInspection), BindingFlags.Static | BindingFlags.NonPublic)));
                        break;
                    }
                }
            }

            return codes;
        }

        private static void HandleInspection(JailbirdItem instance, JailbirdMessageType messageType)
        {
            try
            {
                if (messageType != JailbirdMessageType.Inspect)
                    return;

                if (!Utilities.TryGetSummonedCustomItem(instance.ItemSerial, out var customItem))
                    return;

                customItem.HandleEvent(Player.Get(instance.Owner), ItemEvents.Inspect, instance.ItemSerial);
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(JailbirdInspectionTranspiler)}: {ex}");
            }
        }
    }
}