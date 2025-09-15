using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(LinearAdsModule), "ServerProcessCmd")]
    internal static class LinearAdsModuleServerProcessCmdTranspiler
    {
        // Fixes Exiled spamming the LabAPI PlayerAimedEventArgs event when the player gets a effect
        // Also makes the event trigger when the player stops aiming (whoops)
        private static string text = string.Empty;
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            Type tLinearAds = AccessTools.TypeByName("InventorySystem.Items.Firearms.Modules.LinearAdsModule") ?? typeof(LinearAdsModule);
            Type tFirearmSubcomponent = AccessTools.TypeByName("InventorySystem.Items.Firearms.FirearmSubcomponentBase") ?? typeof(InventorySystem.Items.Firearms.FirearmSubcomponentBase);
            Type tItemBase = AccessTools.TypeByName("InventorySystem.Items.ItemBase") ?? typeof(InventorySystem.Items.ItemBase);
            Type tPlayerArgs = AccessTools.TypeByName("LabApi.Events.Arguments.PlayerEvents.PlayerAimedWeaponEventArgs");
            Type tPlayerEvents = AccessTools.TypeByName("LabApi.Events.Handlers.PlayerEvents");
            Type tReferenceHub = AccessTools.TypeByName("ReferenceHub");
            MethodInfo mGetFirearm = AccessTools.Method(tFirearmSubcomponent, "get_Firearm");
            MethodInfo mGetOwner = AccessTools.Method(tItemBase, "get_Owner");
            FieldInfo fldUserInput = AccessTools.Field(tLinearAds, "_userInput");
            ConstructorInfo ctorPlayerAimed = null;
            MethodInfo mOnAimed = null;

            if (tPlayerArgs != null)
            {
                ctorPlayerAimed = AccessTools.Constructor(tPlayerArgs,
                [
                    tReferenceHub ?? AccessTools.TypeByName("ReferenceHub"),
                    AccessTools.TypeByName("InventorySystem.Items.Firearms.Firearm"),
                    typeof(bool)
                ]);
            }

            if (tPlayerEvents != null && tPlayerArgs != null)
                mOnAimed = AccessTools.Method(tPlayerEvents, "OnAimedWeapon", [tPlayerArgs]);

            if (mGetFirearm == null || mGetOwner == null || fldUserInput == null || ctorPlayerAimed == null || mOnAimed == null)
                return codes.AsEnumerable();

            Predicate<CodeInstruction>[] pattern =
            [
                // ldarg.0
                new Predicate<CodeInstruction>(ci => ci.opcode == OpCodes.Ldarg_0 || ci.opcode == OpCodes.Ldarg),
                // call get_Firearm
                new Predicate<CodeInstruction>(ci => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && ci.operand is MethodInfo mi1 && mi1 == mGetFirearm),
                // callvirt get_Owner
                new Predicate<CodeInstruction>(ci => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && ci.operand is MethodInfo mi2 && mi2 == mGetOwner),
                // ldarg.0
                new Predicate<CodeInstruction>(ci => ci.opcode == OpCodes.Ldarg_0 || ci.opcode == OpCodes.Ldarg),
                // call get_Firearm
                new Predicate<CodeInstruction>(ci => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && ci.operand is MethodInfo mi3 && mi3 == mGetFirearm),
                // ldarg.0
                new Predicate<CodeInstruction>(ci => ci.opcode == OpCodes.Ldarg_0 || ci.opcode == OpCodes.Ldarg),
                // ldfld _userInput
                new Predicate<CodeInstruction>(ci => ci.opcode == OpCodes.Ldfld && ci.operand is FieldInfo fi && fi == fldUserInput),
                // newobj PlayerAimedWeaponEventArgs::.ctor(...)
                new Predicate<CodeInstruction>(ci => ci.opcode == OpCodes.Newobj && ci.operand is ConstructorInfo ctor && ctor == ctorPlayerAimed),
                // call PlayerEvents.OnAimedWeapon
                new Predicate<CodeInstruction>(ci => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && ci.operand is MethodInfo mi4 && mi4 == mOnAimed)
            ];

            int index = codes.FindSequence(pattern);
            if (index >= 0)
                codes.RemoveRange(index, pattern.Length);

            foreach (CodeInstruction code in codes)
                text += "\n" + code.ToString();
            LogManager.Silent($"{nameof(LinearAdsModuleServerProcessCmdTranspiler)}: Codes: {text}");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(LinearAdsModule), "OnAdsChanged")]
    internal static class LinearAdsModuleEventPatch
    {
        private static string text = string.Empty;
        private static readonly FieldInfo UserInputField = AccessTools.Field(typeof(LinearAdsModule), "_userInput");
        private static readonly MethodInfo FireEventMethod = AccessTools.Method(typeof(LinearAdsModuleEventPatch), nameof(FireAdsEvent));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);

            for (int i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode == OpCodes.Ret)
                {
                    List<CodeInstruction> newInstructions =
                    [
                        // This
                        new CodeInstruction(OpCodes.Ldarg_0),
                        // Load targetChanged parameter
                        new CodeInstruction(OpCodes.Ldarg, 3),
                        // Call FireAdsEvent()
                        new CodeInstruction(OpCodes.Call, FireEventMethod)
                    ];

                    codes.InsertRange(i, newInstructions);
                    break;
                }
            }

            foreach (CodeInstruction code in codes)
                text += "\n" + code.ToString();
            LogManager.Silent($"{nameof(LinearAdsModuleEventPatch)}: Codes: {text}");

            return codes;
        }

        private static void FireAdsEvent(LinearAdsModule instance, bool targetChanged)
        {
            if (!instance.IsServer || !targetChanged)
                return;

            try
            {
                bool userInput = (bool)UserInputField.GetValue(instance);
                LogManager.Debug($"{nameof(LinearAdsModuleEventPatch)}: Firing ADS event for item (ADS: {userInput})");
                PlayerEvents.OnAimedWeapon(new PlayerAimedWeaponEventArgs(instance.Firearm.Owner, instance.Firearm, userInput));
            }
            catch (Exception ex)
            {
                LogManager.Debug($"{nameof(LinearAdsModuleEventPatch)}: {ex.Message}");
            }
        }
    }
}
