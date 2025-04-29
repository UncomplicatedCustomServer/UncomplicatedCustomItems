using HarmonyLib;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    internal class ServerNamePatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!Plugin.Instance.Config.ServerTracking)
            {
                LogManager.Debug("ServerTracking in config is set to false aborting ServerName patch.");
                return;
            }
            ServerConsole.ServerName += $"<color=#00000000><size=1>UCI {Plugin.Instance.Version.ToString(3)}</size></color>";
            LogManager.Debug("ServerName Patched!");
        }
    }
}