using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UncomplicatedCustomItems.Commands.Admin;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public static class UpdateChecker
    {
        public static async Task CheckForUpdatesAsync()
        {
            Version version = null;
            try
            {
                version = Plugin.Instance.Version;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Exception while retrieving local plugin version: {ex.Message} \n {ex.StackTrace}");
                return;
            }

            LogManager.Updater($"Current version: {version}. Checking GitHub for latest release...");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "UncomplicatedCustomItems-UpdateChecker/1.0");

                    if (!string.IsNullOrEmpty(Plugin.Instance.Config.GithubToken))
                        client.DefaultRequestHeaders.Add("Authorization", $"token {Plugin.Instance.Config.GithubToken}");

                    string apiUrl = "https://api.github.com/repos/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest";
                    HttpResponseMessage httpResponse = await client.GetAsync(apiUrl);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                        GitHubReleaseInfo latestRelease = JsonConvert.DeserializeObject<GitHubReleaseInfo>(jsonResponse);

                        if (latestRelease != null && !string.IsNullOrEmpty(latestRelease.TagName))
                        {
                            string latestVersionTag = latestRelease.TagName;
                            if (latestVersionTag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                                latestVersionTag = latestVersionTag.Substring(1);

                            if (Version.TryParse(latestVersionTag, out Version githubVersion))
                            {
                                LogManager.Updater($"Latest version on GitHub: {githubVersion} (Tag: {latestRelease.TagName})");
                                if (githubVersion > version)
                                {
                                    LogManager.Updater($"An update is available! Current: {version}, Latest: {githubVersion}");
                                    LogManager.Updater($"Use the 'uciupdate' command to update the plugin.");
                                }
                                else if (githubVersion == version)
                                    LogManager.Updater($"You are running the latest version ({version}).");
                                else
                                    LogManager.Updater($"You are on a dev version ({version}). Latest stable: {githubVersion}.");
                            }
                            else
                                LogManager.Error($"Could not parse GitHub version '{latestVersionTag}'.");
                        }
                        else
                            LogManager.Error("Invalid release info or empty tag name from GitHub.");
                    }
                    else
                    {
                        string errorContent = await httpResponse.Content.ReadAsStringAsync();
                        LogManager.Error($"GitHub API error: {httpResponse.StatusCode}. Response: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Unexpected error during update check: {ex}");
            }
        }
    }

}