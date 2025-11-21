using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    internal static class VersionManager
    {
        public static VersionInfoV2 VersionInfo { get; set; }

#nullable enable
        public static async void Init()
        {
            Tuple<HttpStatusCode, string?> data = await Plugin.HttpManager.VersionInfo();

            if (data.Item1 is not HttpStatusCode.OK || data.Item2 is null)
            {
                LogManager.Warn($"Failed to gain the current version info from our central servers: API endpoint says {data.Item1}");
                return;
            }

            VersionInfo = JsonConvert.DeserializeObject<VersionInfoV2>(data.Item2);

            if (VersionInfo is null)
            {
                LogManager.Silent($"Failed to convert API endpoint answer to VersionInfo.\nContent: {data.Item2}");
                return;
            }

            if (VersionInfo.PreRelease)
            {
                LogManager.Info($"NOTICE!\nYou are currently using version v{Plugin.Instance.Version.ToString(3)}, which is a PRE-RELEASE or EXPERIMENTAL RELEASE of UncomplicatedCustomItems. Latest stable release: {Plugin.HttpManager.LatestVersion} NOTE: This is NOT a stable version, so it may contain bugs and errors. For this reason, its use in production is not recommended.");
                if (VersionInfo.ForceDebug)
                {
                    LogManager.Info("Debug logs have been activated!");
                    Plugin.Instance.Config.Debug = true;
                }
            }
            else
                LogManager.Info($"You are using UncomplicatedCustomItems v{VersionInfo.Version}{(VersionInfo.CodeName is not null ? $" '{VersionInfo.CodeName}'" : string.Empty)}!");

            if (VersionInfo is { Recalled: true, RecallReason: not null })
            {
                LogManager.Security($"This version has been recalled! Due to {VersionInfo.RecallReason} Please update the plugin to keep using it");
#if EXILED
                Plugin.Instance.OnDisabled();
#else
                Plugin.Instance.Disable();
#endif
            }
        }

        public static string HashFile(string path)
        {
            FileStream file = new(path, FileMode.Open)
            {
                Position = 0
            };

            byte[] bytes = SHA256.Create().ComputeHash(file);
            file.Close();
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}