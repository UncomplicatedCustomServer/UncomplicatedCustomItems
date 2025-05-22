using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace UncomplicatedCustomItems.Extensions
{
    public static class HarmonyExtensions
    {
        /// <summary>
        /// Finds the index of the first instruction that matches the predicate
        /// </summary>
        public static int FindIndex(this List<CodeInstruction> instructions, Predicate<CodeInstruction> match)
        {
            return instructions.FindIndex(match);
        }

        /// <summary>
        /// Finds all indices where instructions match the predicate
        /// </summary>
        public static List<int> FindAllIndices(this List<CodeInstruction> instructions, Predicate<CodeInstruction> match)
        {
            var indices = new List<int>();
            for (int i = 0; i < instructions.Count; i++)
            {
                if (match(instructions[i]))
                    indices.Add(i);
            }
            return indices;
        }

        /// <summary>
        /// Replaces instructions at specified index with new instructions
        /// </summary>
        public static void ReplaceAt(this List<CodeInstruction> instructions, int index, params CodeInstruction[] newInstructions)
        {
            instructions.RemoveAt(index);
            instructions.InsertRange(index, newInstructions);
        }

        /// <summary>
        /// Inserts instructions after the first occurrence matching the predicate
        /// </summary>
        public static void InsertAfter(this List<CodeInstruction> instructions, Predicate<CodeInstruction> match, params CodeInstruction[] newInstructions)
        {
            int index = instructions.FindIndex(match);
            if (index >= 0)
            {
                instructions.InsertRange(index + 1, newInstructions);
            }
        }

        /// <summary>
        /// Inserts instructions before the first occurrence matching the predicate
        /// </summary>
        public static void InsertBefore(this List<CodeInstruction> instructions, Predicate<CodeInstruction> match, params CodeInstruction[] newInstructions)
        {
            int index = instructions.FindIndex(match);
            if (index >= 0)
            {
                instructions.InsertRange(index, newInstructions);
            }
        }

        /// <summary>
        /// Creates a sequence of instructions to call a static method
        /// </summary>
        public static CodeInstruction[] CallStatic(MethodInfo method, params CodeInstruction[] loadInstructions)
        {
            var result = new List<CodeInstruction>(loadInstructions);
            result.Add(new CodeInstruction(OpCodes.Call, method));
            return result.ToArray();
        }

        /// <summary>
        /// Creates instructions to load a constant value
        /// </summary>
        public static CodeInstruction LoadConstant(object value)
        {
            return value switch
            {
                null => new CodeInstruction(OpCodes.Ldnull),
                int i => new CodeInstruction(OpCodes.Ldc_I4, i),
                float f => new CodeInstruction(OpCodes.Ldc_R4, f),
                double d => new CodeInstruction(OpCodes.Ldc_R8, d),
                string s => new CodeInstruction(OpCodes.Ldstr, s),
                bool b => new CodeInstruction(OpCodes.Ldc_I4, b ? 1 : 0),
                _ => throw new ArgumentException($"Unsupported constant type: {value.GetType()}")
            };
        }

        /// <summary>
        /// Finds a sequence of instructions matching the pattern
        /// </summary>
        public static int FindSequence(this List<CodeInstruction> instructions, params Predicate<CodeInstruction>[] pattern)
        {
            for (int i = 0; i <= instructions.Count - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (!pattern[j](instructions[i + j]))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }
    }
}