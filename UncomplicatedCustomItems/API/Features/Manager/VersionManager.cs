using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    internal static class VersionManager
    {
        public static VersionInfoV2? VersionInfo { get; set; }
        
        public static void Init()
        {
            Plugin.HttpManager.VersionInfo((status, content) =>
            {
                if (status is not HttpStatusCode.OK || content is null)
                {
                    LogManager.Warn($"Failed to gain the current version info from our central servers: API endpoint says {status}");
                    return;
                }

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true
                };

                try
                {
                    VersionInfo = JsonSerializer.Deserialize<VersionInfoV2>(content, options);

                    if (VersionInfo is null)
                    {
                        LogManager.Silent($"Failed to convert API endpoint answer to VersionInfo.\nContent: {content}");
                        return;
                    }

                    if (VersionInfo.PreRelease)
                    {
                        LogManager.Info(
                            $"NOTICE!\nYou are currently using version v{Plugin.Instance.Version.ToString(3)}, " +
                            $"which is a PRE-RELEASE or EXPERIMENTAL RELEASE of UncomplicatedCustomItems. " +
                            $"Latest stable release: {Plugin.HttpManager.LatestVersion} " +
                            $"NOTE: This is NOT a stable version, so it may contain bugs and errors. " +
                            $"For this reason, its use in production is not recommended.");

                        if (VersionInfo.ForceDebug)
                        {
                            LogManager.Info("Debug logs have been activated!");
                            Plugin.Instance.Config?.Debug = true;
                        }
                    }
                    else
                        LogManager.Info($"You are using UncomplicatedCustomItems v{VersionInfo.Version} {(VersionInfo.CodeName is not null ? $" '{VersionInfo.CodeName}'" : string.Empty)}!");

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
                catch (JsonException ex)
                {
                    LogManager.Error($"Failed to parse version info JSON: {ex.Message}\nContent: {content}");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Unexpected error while processing version info: {ex}");
                }
            });
        }

        public static string HashFile(string path)
        {
            using FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bytes = SHA256.Create().ComputeHash(file);
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}