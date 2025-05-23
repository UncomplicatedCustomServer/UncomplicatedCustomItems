using CommandSystem;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class GitHubReleaseInfo
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("assets")]
        public GitHubAssetInfo[] Assets { get; set; }
    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class UpdateCheck : ParentCommand
    {
        public UpdateCheck() => LoadGeneratedCommands();

        public override string Command { get; } = "uciupdatecheck";
        public override string[] Aliases { get; } = new string[] { "ucicheckupdate" };
        public override string Description { get; } = "Checks if a new version of UncomplicatedCustomItems is available.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Version version = null;
            try
            {
                version = Plugin.Instance.Version;
            }
            catch (Exception ex)
            {
                response = $"Error: Could not retrieve local plugin version. Details: {ex.Message}";
                LogManager.Error($"Exception while retrieving local plugin version: {ex}");
                return false;
            }

            response = $"Currently running UncomplicatedCustomItems version {version}. Checking for updates...";
            LogManager.Raw($"Current version: {version}. Checking GitHub for latest release...", ConsoleColor.Blue);

            Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "UncomplicatedCustomItems-UpdateChecker/1.0");
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
                                    LogManager.Raw($"Latest version on GitHub: {githubVersion} (Tag: {latestRelease.TagName})", ConsoleColor.Blue);
                                    if (githubVersion > version)
                                    {
                                        LogManager.Raw($"An update is available for UncomplicatedCustomItems!", ConsoleColor.Blue);
                                        LogManager.Raw($"Current version: {version}, Latest version: {githubVersion}.", ConsoleColor.Blue);
                                        LogManager.Raw($"Please use the 'uciupdate' command to update the plugin.", ConsoleColor.Blue);
                                    }
                                    else if (githubVersion == version)
                                        LogManager.Raw($"You are running the latest version of UncomplicatedCustomItems ({version}).", ConsoleColor.Blue);
                                    else
                                        LogManager.Raw($"You are running a newer version ({version}) than the latest stable release on GitHub. This is a development or pre-release version.", ConsoleColor.Blue);
                                }
                                else
                                    LogManager.Error($"Failed to parse the latest version tag '{latestVersionTag}' from GitHub into a valid version format.");
                            }
                            else
                                LogManager.Error("Failed to parse release information from GitHub or tag name was empty.");
                        }
                        else
                        {
                            string errorContent = await httpResponse.Content.ReadAsStringAsync();
                            LogManager.Error($"Failed to fetch latest release from GitHub. Status code: {httpResponse.StatusCode}. Response: {errorContent}");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogManager.Error($"A network error occurred while checking for updates: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    LogManager.Error($"Error parsing JSON response from GitHub: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"An unexpected error occurred: {ex.ToString()}");
                }
            });
            return true;
        }
    }
}
