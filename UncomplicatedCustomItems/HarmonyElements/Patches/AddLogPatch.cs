using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using UncomplicatedCustomItems.API.Features.Networking;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public class AddLogPatch
    {
        [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.PrintFormattedString))]
        public static void Postfix(string text, ConsoleColor defaultColor)
        {
            if (!text.Contains("UncomplicatedCustomItems") || !text.Contains("[ERROR]") || !Plugin.Instance.Config.AutomaticErrorUpload)
                return;

            text = Regex.Replace(text, @"_Patch\d+", "");
            string filtered = text.Replace("MonoMod.Utils.DynamicMethodDefinition.", "");
            AutoLogRequest request = new(filtered, "UCI", "ERROR");
            request.SendRequest();
        }
    }
}
