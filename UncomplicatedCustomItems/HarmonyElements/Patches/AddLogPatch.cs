using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using UncomplicatedCustomItems.API.Features.Networking;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public class AddLogPatch
    {
        private static readonly Regex PatchRegex = new(@"_Patch\d+", RegexOptions.Compiled);

        [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.PrintFormattedString))]
        public static void Postfix(string text, ConsoleColor defaultColor)
        {
            if (!Plugin.Instance.Config.AutomaticErrorUpload || !text.Contains("UncomplicatedCustomItems") || !text.Contains("[ERROR]"))
                return;

            text = PatchRegex.Replace(text, "");
            string filtered = text.Replace("MonoMod.Utils.DynamicMethodDefinition.", "");
            AutoLogRequest request = new(filtered, "UCI", "ERROR");
            request.SendRequest();
        }
    }
}
