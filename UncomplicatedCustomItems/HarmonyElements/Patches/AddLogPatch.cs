using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using UncomplicatedCustomItems.API.Features.Networking;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch]
    public class AddLogPatch
    {
        [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
        public static void Postfix(string q, ConsoleColor color, bool hideFromOutputs)
        {
            if (!q.Contains("UncomplicatedCustomItems") || !q.Contains("[ERROR]") || !Plugin.Instance.Config.AutomaticErrorUpload)
                return;

            q = Regex.Replace(q, @"_Patch\d+", "");
            string filtered = q.Replace("MonoMod.Utils.DynamicMethodDefinition.", "");
            AutoLogRequest request = new(filtered, "UCI", "ERROR");
            request.SendRequest();
        }
    }
}
