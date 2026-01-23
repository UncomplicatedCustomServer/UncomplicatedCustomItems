#if EXILED
using Exiled.API.Features;
#endif
using System.Text.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomItems.Commands;
using UnityEngine.Networking;
using System.Text.Json;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class Updater
    {
        public class GitHubReleaseInfo
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; }

            [JsonPropertyName("prerelease")]
            public bool PreRelease { get; set; }

            [JsonPropertyName("assets")]
            public GitHubAssetInfo[] Assets { get; set; }

            [JsonPropertyName("body")]
            public string Body { get; set; }
        }

        public class GitHubAssetInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
        
#if EXILED
        private const string PluginDllName = "UncomplicatedCustomItems-Exiled.dll";
#else
        private const string PluginDllName = "UncomplicatedCustomItems-LabApi.dll";
#endif
        private const string UserAgent = "UncomplicatedCustomItems-Updater/1.2";
        private const string ReleasesApiUrl = "https://api.github.com/repos/UncomplicatedCustomServer/UncomplicatedCustomItems/releases";

        public static IEnumerator CheckForUpdatesCoroutine()
        {
            Version currentVersion = Plugin.Instance.Version;
            LogManager.Updater($"Current version: {currentVersion}. Checking for updates...");

            GitHubReleaseInfo latestRelease = null;
            yield return GetLatestReleaseCoroutine(result => latestRelease = result);

            if (latestRelease == null)
                yield break;

            string latestVersionTag = latestRelease.TagName?.TrimStart('v') ?? string.Empty;
            if (Version.TryParse(latestVersionTag, out Version githubVersion))
            {
                LogManager.Updater($"Latest version: {githubVersion}.");
                if (githubVersion > currentVersion)
                {
                    LogManager.Updater("An update is available! Use the 'uciupdate' command to install it."); 
                    if (!string.IsNullOrWhiteSpace(latestRelease.Body))
                        LogManager.Updater($"Changes: \n {latestRelease.Body}");
                }
                else if (githubVersion < currentVersion)
                {
                    LogManager.Updater("You are on a Pre Release or Developer version! :D");
                }
                else
                    LogManager.Updater("You are on the latest version.");
            }
        }

        public static IEnumerator UpdatePluginCoroutine(Version currentVersion, string forceArgument)
        {
            GitHubReleaseInfo latestRelease = null;
            yield return GetLatestReleaseCoroutine(result => latestRelease = result);

            if (latestRelease == null)
                yield break;

            GitHubAssetInfo asset = latestRelease.Assets?.FirstOrDefault(a => a.Name.Equals(PluginDllName, StringComparison.OrdinalIgnoreCase));

            if (asset == null || string.IsNullOrEmpty(asset.BrowserDownloadUrl))
            {
                LogManager.Error($"Could not find the plugin DLL ('{PluginDllName}') in the latest GitHub release.");
                yield break;
            }

            string latestVersionTag = latestRelease.TagName?.TrimStart('v') ?? string.Empty;
            if (Version.TryParse(latestVersionTag, out Version latestGitHubVersion) && latestGitHubVersion <= currentVersion && string.Equals(forceArgument, "force", StringComparison.OrdinalIgnoreCase) == false)
            {
                LogManager.Updater("You are already on the latest version. Use 'uciupdate force' to proceed anyway.");
                yield break;
            }

            LogManager.Updater($"Downloading new version from {asset.BrowserDownloadUrl}...");

            if (!string.IsNullOrWhiteSpace(latestRelease.Body))
                LogManager.Updater($"Changes for this version: \n {latestRelease.Body}");

            UnityWebRequest req = UnityWebRequest.Get(asset.BrowserDownloadUrl);
            req.SetRequestHeader("User-Agent", UserAgent);
            if (!string.IsNullOrEmpty(Plugin.Instance.Config.GithubToken))
                req.SetRequestHeader("Authorization", $"token {Plugin.Instance.Config.GithubToken}");

            req.downloadHandler = new DownloadHandlerBuffer();
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                LogManager.Error($"Failed to download plugin: {req.error} ({req.responseCode})");
                yield break;
            }

            try
            {
                byte[] fileBytes = req.downloadHandler.data ?? Array.Empty<byte>();
                LogManager.Updater($"{PluginDllName} downloaded successfully ({fileBytes.Length} bytes). Applying update...");

                File.WriteAllBytes(GetPluginPath(), fileBytes);
                LabApi.Features.Wrappers.Server.RunCommand("rnr", new SilentCommandSender());
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to write plugin file: {ex}");
            }
        }

        private static IEnumerator GetLatestReleaseCoroutine(Action<GitHubReleaseInfo> onComplete)
        {
            UnityWebRequest req = UnityWebRequest.Get(ReleasesApiUrl);
            req.SetRequestHeader("User-Agent", UserAgent);
            req.SetRequestHeader("Accept", "application/vnd.github.v3+json");
            if (!string.IsNullOrEmpty(Plugin.Instance.Config.GithubToken))
                req.SetRequestHeader("Authorization", $"token {Plugin.Instance.Config.GithubToken}");

            req.downloadHandler = new DownloadHandlerBuffer();
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                LogManager.Error($"Failed to fetch release info from GitHub. Error: {req.error} ({req.responseCode})");
                onComplete?.Invoke(null);
                yield break;
            }

            try
            {
                string jsonResponse = req.downloadHandler.text;
                List<GitHubReleaseInfo> releases = JsonSerializer.Deserialize<List<GitHubReleaseInfo>>(jsonResponse);

                if (releases == null || releases.Count == 0)
                {
                    onComplete?.Invoke(null);
                    yield break;
                }

                IEnumerable<GitHubReleaseInfo> filtered = Plugin.Instance.Config.AllowPreReleases ? releases : releases.Where(r => !r.PreRelease);

                GitHubReleaseInfo chosen = filtered.OrderByDescending(r =>
                {
                    string tag = r.TagName?.TrimStart('v') ?? string.Empty;
                    return Version.TryParse(tag, out Version v) ? v : new Version(0, 0);
                }).FirstOrDefault();
                onComplete?.Invoke(chosen);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error parsing GitHub response: {ex.Message}");
                onComplete?.Invoke(null);
            }
        }

        private static string GetPluginPath()
        {
#if EXILED
            return Path.Combine(Paths.Plugins, PluginDllName);
#else
            return Plugin.Instance.FilePath;
#endif
        }
    }
}