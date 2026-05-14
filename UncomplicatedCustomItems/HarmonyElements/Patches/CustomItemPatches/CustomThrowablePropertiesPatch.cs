using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Features.SpecificData;
using Scp018Projectile = InventorySystem.Items.ThrowableProjectiles.Scp018Projectile;
using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(ThrowableItem), nameof(ThrowableItem.ServerThrow))]
    internal static class CustomThrowablePropertiesPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = new(instructions);

            try
            {
                // Find the last ret instruction
                int returnIndex = -1;
                for (int i = newInstructions.Count - 1; i >= 0; i--)
                {
                    if (newInstructions[i].opcode == OpCodes.Ret)
                    {
                        returnIndex = i;
                        break;
                    }
                }

                if (returnIndex == -1)
                {
                    LogManager.Error("CustomThrowablePropertiesPatch: Could not find return instruction");
                    return instructions;
                }

                // Find the variable that holds ThrownProjectile
                // Look for the last instruction that loads from a local before the return
                LocalBuilder projectileLocal = null!;
                for (int i = returnIndex - 1; i >= 0; i--)
                {
                    CodeInstruction instruction = newInstructions[i];

                    // Look for ldloc instructions
                    if (instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Ldloc_1 || instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Ldloc_3 || instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloc)
                    {
                        if (instruction.operand is LocalBuilder lb)
                        {
                            projectileLocal = lb;
                            break;
                        }

                        int localIndex = instruction.opcode.Name switch
                        {
                            "ldloc.0" => 0,
                            "ldloc.1" => 1,
                            "ldloc.2" => 2,
                            "ldloc.3" => 3,
                            _ => -1
                        };

                        if (localIndex >= 0)
                        {
                            projectileLocal = generator.DeclareLocal(typeof(ThrownProjectile));
                            projectileLocal.SetLocalSymInfo($"local_{localIndex}");
                            break;
                        }
                    }
                }

                if (projectileLocal == null)
                {
                    LogManager.Error("CustomThrowablePropertiesPatch: Could not find ThrownProjectile local variable");
                    return instructions;
                }

                // Insert the code right before the return
                int insertIndex = returnIndex;

                // Dup the return value (ThrownProjectile on stack)
                newInstructions.Insert(insertIndex++, new CodeInstruction(OpCodes.Dup));

                // Load 'this' (ThrowableItem)
                newInstructions.Insert(insertIndex++, new CodeInstruction(OpCodes.Ldarg_0));

                // Swap the two values on stack (ThrowableItem, ThrownProjectile order)
                newInstructions.Insert(insertIndex++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CustomThrowablePropertiesPatch), nameof(ApplyCustomProperties))));

                LogManager.Debug("CustomThrowablePropertiesPatch: Successfully patched ServerThrow");
                return newInstructions;
            }
            catch (Exception ex)
            {
                LogManager.Error($"CustomThrowablePropertiesPatch: Transpiler failed - {ex}");
                return instructions;
            }
        }

        private static void ApplyCustomProperties(ThrownProjectile thrownProjectile, ThrowableItem throwableItem)
        {
            try
            {
                if (thrownProjectile == null || throwableItem == null)
                    return;

                if (Utilities.TryGetSummonedCustomItem(throwableItem.ItemSerial, out var summoned) && summoned != null)
                {
                    switch (summoned.CustomItem.CustomItemType)
                    {
                        case CustomItemType.ExplosiveGrenade when summoned.CustomItem.CustomData is ExplosiveGrenadeData exdata && thrownProjectile is ExplosionGrenade exGrenade:
                            if (thrownProjectile is TimeGrenade timedGrenade)
                                timedGrenade._fuseTime = exdata.FuseTime;

                            exGrenade.MaxRadius = exdata.MaxRadius;
                            exGrenade.ScpDamageMultiplier = exdata.ScpDamageMultiplier;
                            exGrenade._burnedDuration = exdata.BurnDuration;
                            exGrenade._concussedDuration = exdata.ConcussDuration;
                            exGrenade._deafenedDuration = exdata.DeafenDuration;
                            exGrenade._doorDamageOverDistance.Multiply(exdata.DoorDamageMultiplier);
                            exGrenade._playerDamageOverDistance.Multiply(exdata.PlayerDamageMultiplier);

                            if (exdata.ExplodeOnImpact)
                                exGrenade.gameObject.AddComponent<CollisionHandler>().Init(exGrenade.gameObject, exGrenade);

                            break;

                        case CustomItemType.FlashGrenade when summoned.CustomItem.CustomData is FlashGrenadeData flashdata && thrownProjectile is FlashbangGrenade flash:
                            if (thrownProjectile is TimeGrenade timedGrenade1)
                                timedGrenade1._fuseTime = flashdata.FuseTime;

                            flash.BlindTime = flashdata.AdditionalBlindedEffect;
                            flash._minimalEffectDuration = flashdata.MinimalDurationEffect;
                            flash._additionalBlurDuration = flashdata.AdditionalBlindedEffect;
                            flash._surfaceZoneDistanceIntensifier = flashdata.SurfaceDistanceIntensifier;

                            if (flashdata.ExplodeOnImpact)
                                flash.gameObject.AddComponent<CollisionHandler>().Init(flash.gameObject, flash);

                            break;

                        case CustomItemType.SCPItem when summoned.CustomItem.CustomData is SCP018Data scp018data && thrownProjectile is Scp018Projectile scp018:
                            if (thrownProjectile is TimeGrenade timedGrenade2)
                                timedGrenade2._fuseTime = scp018data.FuseTime;

                            scp018._friendlyFireTime = scp018data.FriendlyFireTime;

                            if (scp018data.ExplodeOnImpact)
                                scp018.gameObject.AddComponent<CollisionHandler>().Init(scp018.gameObject, scp018);

                            break;

                        default:
                            LogManager.Warn($"Unsupported ItemType {throwableItem.ItemTypeId} was thrown as a projectile");
                            break;
                    }
                }
                else if (SummonedAPICustomItem.TryGet(throwableItem.ItemSerial, out var api) && api != null)
                {
                    switch (api.CustomItem)
                    {
                        case CustomExplosiveGrenade exdata when thrownProjectile is ExplosionGrenade exGrenade:
                            if (thrownProjectile is TimeGrenade projectile)
                                projectile._fuseTime = exdata.FuseTime;

                            exGrenade.MaxRadius = exdata.MaxRadius;
                            exGrenade.ScpDamageMultiplier = exdata.ScpDamageMultiplier;
                            exGrenade._burnedDuration = exdata.BurnDuration;
                            exGrenade._concussedDuration = exdata.ConcussDuration;
                            exGrenade._deafenedDuration = exdata.DeafenDuration;
                            exGrenade._doorDamageOverDistance.Multiply(exdata.DoorDamageMultiplier);
                            exGrenade._playerDamageOverDistance.Multiply(exdata.PlayerDamageMultiplier);
                            if (exdata.ExplodeOnImpact)
                                exGrenade.gameObject.AddComponent<CollisionHandler>().Init(exGrenade.gameObject, exGrenade);

                            break;

                        case CustomFlashGrenade flashdata when thrownProjectile is FlashbangGrenade flash:
                            if (thrownProjectile is TimeGrenade projectile1)
                                projectile1._fuseTime = flashdata.FuseTime;

                            flash.BlindTime = flashdata.AdditionalBlindedEffect;
                            flash._minimalEffectDuration = flashdata.MinimalDurationEffect;
                            flash._additionalBlurDuration = flashdata.AdditionalBlindedEffect;
                            flash._surfaceZoneDistanceIntensifier = flashdata.SurfaceDistanceIntensifier;
                            if (flashdata.ExplodeOnImpact)
                                flash.gameObject.AddComponent<CollisionHandler>().Init(flash.gameObject, flash);

                            break;

                        case CustomSCP018 scp018data when thrownProjectile is Scp018Projectile scp018:
                            if (thrownProjectile is TimeGrenade projectile2)
                                projectile2._fuseTime = scp018data.FuseTime;

                            scp018._friendlyFireTime = scp018data.FriendlyFireTime;
                            scp018._fuseTime = scp018data.FuseTime;
                            if (scp018data.ExplodeOnImpact)
                                scp018.gameObject.AddComponent<CollisionHandler>().Init(scp018.gameObject, scp018);

                            break;

                        default:
                            LogManager.Warn($"Unsupported ItemType {throwableItem.ItemTypeId} was thrown as a projectile");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"CustomThrowablePropertiesPatch.ApplyCustomProperties failed: {ex}");
            }
        }
    }
}