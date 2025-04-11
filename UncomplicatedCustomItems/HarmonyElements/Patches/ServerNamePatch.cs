using HarmonyLib;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.HarmonyElements.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    internal class ServerNamePatch
    {
        public static void Postfix()
        {
            if (!Plugin.Instance.Config.ServerTracking)
            {
                LogManager.Debug("ServerTracking in config is set to false aborting ServerName patch.");
                return;
            }

            FieldInfo ServerName = AccessTools.Field(typeof(ServerConsole), "_serverName");
            string CurrentName = (string)ServerName.GetValue(null);
            ServerName.SetValue(null, CurrentName + $"<color=#00000000><size=1>UCI {Plugin.Instance.Version.ToString(3)}</size></color>");
            LogManager.Debug("ServerName Patched!");
        }
    }
}