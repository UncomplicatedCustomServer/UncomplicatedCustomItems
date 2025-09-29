using HarmonyLib;
using InventorySystem.Items.Usables;
using System.Collections.Generic;
using System.Reflection.Emit;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches.CustomItemPatches
{
    [HarmonyPatch(typeof(Scp268), nameof(Scp268.ServerOnUsingCompleted))]
    public static class SCP268CooldownTimeTranspiler
    {
        private static string text = string.Empty;

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float floatValue && floatValue == 120f)
                {
                    // Replace the constant with a call to GetCooldownTime()
                    codes[i] = new CodeInstruction(OpCodes.Ldarg_0); // This
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SCP268CooldownTimeTranspiler), nameof(GetCooldownTime))));
                    break;
                }
            }

           foreach (CodeInstruction code in codes)
                text += "\n" + code.ToString();
            LogManager.Silent($"{nameof(SCP268CooldownTimeTranspiler)}: Codes: {text}");
             
            return codes;
        }

        public static float GetCooldownTime(Scp268 instance)
        {
            if (Utilities.TryGetSummonedCustomItem(instance.ItemSerial, out var item) && item.CustomItem.CustomItemType is CustomItemType.SCPItem && item.CustomItem.CustomData is SCP268Data data)
            {
                LogManager.Debug($"{nameof(SCP268CooldownTimeTranspiler)}: Total cooldown: {data.Cooldown}");
                return data.Cooldown;
            }
            if (SummonedAPICustomItem.TryGet(instance.ItemSerial, out var baseitem) && baseitem.CustomItem is CustomSCP268 customSCP268)
            {
                LogManager.Debug($"{nameof(SCP268CooldownTimeTranspiler)}: Total cooldown: {customSCP268.Cooldown}");
                return customSCP268.Cooldown;
            }

            return 120f;
        }
    }
}