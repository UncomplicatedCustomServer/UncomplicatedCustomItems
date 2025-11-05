using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Modules;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(AutomaticActionModule))]
    internal static class AutomaticActionModulePatch
    {
        [HarmonyPatch("get_ChamberSize")]
        [HarmonyPrefix]
        public static bool Prefix(AutomaticActionModule __instance, ref int __result)
        {
            if (!Utilities.TryGetSummonedCustomItem(__instance.Firearm.ItemSerial, out var customItem) || !SummonedAPICustomItem.TryGet(__instance.ItemSerial, out var summonItem))
                return true;
            if (customItem.CustomItem.CustomItemType is not CustomItemType.Weapon || summonItem.CustomItem is not CustomWeapon customWeapon)
                return true;

            if (customItem != null)
            {
                IWeaponData weaponData = customItem.CustomItem.CustomData as IWeaponData;
                __result = weaponData.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }
            else if (summonItem != null)
            {
                __result = customWeapon.MaxBarrelAmmo;
                __instance.ServerResync();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Transpiler for <see cref="AutomaticActionModule.UpdateServer"/> that prevents
        /// <see cref="CustomItemsAPI.CustomItems"/> firearms from being blocked by the module 'busy' state check.
        /// <para>
        /// In vanilla logic, the firearm’s firing sequence is halted if module 5 (the busy module)
        /// reports a busy state. This patch injects a check to skip that restriction when the firearm
        /// belongs to a registered CustomItem, allowing it to fire normally.
        /// </para>
        /// </summary>
        [HarmonyPatch(nameof(AutomaticActionModule.UpdateServer))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateServerTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new(instructions);
            for (int i = 0; i < codes.Count - 10; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo method && method.Name == "get_Modules")
                {
                    Label skipLabel = generator.DefineLabel();

                    // Insert the custom check before the loop
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
                        if (codes[j].opcode == OpCodes.Callvirt && codes[j].operand is MethodInfo m2 && (m2.Name == "get_AmmoStored" || m2.Name == "get_OpenBolt"))
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