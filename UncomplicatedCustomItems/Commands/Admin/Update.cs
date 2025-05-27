using CommandSystem;
using System.Threading.Tasks;
using System;
using UncomplicatedCustomItems.API.Features.Helper;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.InteropServices;
using Exiled.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class GitHubAssetInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Update : ParentCommand
    {
        private const string PluginDllName = "UncomplicatedCustomItems-LabApi.dll";

        public Update() => LoadGeneratedCommands();

        public override string Command { get; } = "uciupdate";
        public override string[] Aliases { get; } = new string[] { "uciselfupdate" };
        public override string Description { get; } = "Downloads and installs the latest version of UncomplicatedCustomItems, then restarts the server round.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Version version = Plugin.Instance.Version;
            LogManager.Info($"Current UncomplicatedCustomItems version: {version}. Attempting to update...");
            response = $"Attempting to update UncomplicatedCustomItems from version {version}. Check console for details.";

            Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "UncomplicatedCustomItems-Updater/1.0");
                        string apiUrl = "https://api.github.com/repos/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest";
                        HttpResponseMessage httpResponse = await client.GetAsync(apiUrl);

                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            string errorContent = await httpResponse.Content.ReadAsStringAsync();
                            LogManager.Error($"Failed to fetch latest release info from GitHub. Status: {httpResponse.StatusCode}. Response: {errorContent}");
                            return;
                        }

                        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                        GitHubReleaseInfo latestRelease = JsonConvert.DeserializeObject<GitHubReleaseInfo>(jsonResponse);

                        if (latestRelease == null || latestRelease.Assets == null || !latestRelease.Assets.Any())
                        {
                            LogManager.Error("Failed to parse release information or no assets found in the latest release.");
                            return;
                        }

                        GitHubAssetInfo asset = latestRelease.Assets.FirstOrDefault(asset => asset.Name.Equals(PluginDllName, StringComparison.OrdinalIgnoreCase));

                        if (asset == null || string.IsNullOrEmpty(asset.BrowserDownloadUrl))
                        {
                            LogManager.Error($"Could not find the plugin DLL ('{PluginDllName}') in the latest GitHub release assets, or download URL is missing.");
                            return;
                        }

                        string latestVersionTag = latestRelease.TagName;
                        if (latestVersionTag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            latestVersionTag = latestVersionTag.Substring(1);

                        if (Version.TryParse(latestVersionTag, out Version latestGitHubVersion))
                        {
                            LogManager.Raw($"Latest version on GitHub: {latestGitHubVersion} (Tag: {latestRelease.TagName}). Current version: {version}.", ConsoleColor.Blue);
                            if (latestGitHubVersion <= version && arguments.FirstOrDefault()?.ToLower() != "force")
                            {
                                LogManager.Raw($"You are already running version {version} or newer. To force update, use 'uciupdate force'.", ConsoleColor.Blue);
                                return;
                            }
                        }
                        else
                            LogManager.Warn($"Could not parse latest GitHub version tag '{latestRelease.TagName}'. Proceeding with download if forced or newer by asset name.");


                        LogManager.Raw($"Downloading {PluginDllName} from {asset.BrowserDownloadUrl}...", ConsoleColor.Blue);
                        byte[] fileBytes = await client.GetByteArrayAsync(asset.BrowserDownloadUrl);

                        if (fileBytes == null || fileBytes.Length == 0)
                        {
                            LogManager.Error("Downloaded file is empty.");
                            return;
                        }

                        string pluginPath = string.Empty;
                        ushort serverPort = 0;
                        serverPort = Server.Port;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            try
                            {
                                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                                if (string.IsNullOrEmpty(homeDirectory))
                                    LogManager.Error("Could not determine user home directory for Linux path.");
                                else
                                {
                                    string portSpecificLinuxPath = Path.Combine(homeDirectory, ".config", "SCP Secret Laboratory", "LabAPI", "plugins", serverPort.ToString(), PluginDllName);
                                    if (File.Exists(portSpecificLinuxPath))
                                        pluginPath = portSpecificLinuxPath;
                                    else
                                    {
                                        LogManager.Warn($"Linux LabAPI Port-Specific plugin path not found: {portSpecificLinuxPath}. Trying global LabAPI path.");
                                        string globalLinuxPath = Path.Combine(homeDirectory, ".config", "SCP Secret Laboratory", "LabAPI", "plugins", PluginDllName);
                                        if (File.Exists(globalLinuxPath))
                                            pluginPath = globalLinuxPath;
                                        else
                                            LogManager.Warn($"Linux LabAPI Global plugin path not found: {globalLinuxPath}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"Error determining Linux LabAPI plugin paths: {ex.Message}");
                            }
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            try
                            {
                                string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                if (string.IsNullOrEmpty(appDataDirectory))
                                    LogManager.Error("Could not determine AppData directory for Windows path.");
                                else
                                {
                                    string portSpecificWindowsPath = Path.Combine(appDataDirectory, "SCP Secret Laboratory", "LabAPI", "plugins", serverPort.ToString(), PluginDllName);
                                    if (File.Exists(portSpecificWindowsPath))
                                        pluginPath = portSpecificWindowsPath;
                                    else
                                    {
                                        LogManager.Warn($"Windows LabAPI Port-Specific plugin path not found: {portSpecificWindowsPath}. Trying global LabAPI path.");
                                        string globalWindowsPath = Path.Combine(appDataDirectory, "SCP Secret Laboratory", "LabAPI", "plugins", PluginDllName);
                                        if (File.Exists(globalWindowsPath))
                                            pluginPath = globalWindowsPath;
                                        else
                                            LogManager.Warn($"Windows LabAPI Global plugin path not found: {globalWindowsPath}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogManager.Error($"Error determining Windows LabAPI plugin paths: {ex.Message}");
                            }
                        }

                        if (string.IsNullOrEmpty(pluginPath))
                        {
                            LogManager.Error("Could not determine the path of the current plugin DLL using LabAPI paths. Update aborted.");
                            return;
                        }

                        LogManager.Raw("Attempting to overwrite plugin DLL. The server will attempt to restart the round after this.", ConsoleColor.Blue);

                        try
                        {
                            File.WriteAllBytes(pluginPath, fileBytes);
                            LogManager.Raw($"{PluginDllName} downloaded and replaced successfully ({fileBytes.Length} bytes).", ConsoleColor.Blue);
                            LogManager.Raw("Executing 'rnr' command to reload plugins and restart the round...", ConsoleColor.Blue);
                            Server.ExecuteCommand("rnr");
                        }
                        catch (IOException ex)
                        {
                            LogManager.Error($"IO Error writing plugin file: {ex.Message}. Ensure the server has write permissions and the file is not locked. A manual restart might be required after placing the DLL.");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            LogManager.Error($"Unauthorized Access Error writing plugin file: {ex.Message}. Ensure the server process has write permissions to the plugins directory.");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error($"Error saving plugin DLL or executing rnr: {ex.ToString()}");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogManager.Error($"A network error occurred during update: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    LogManager.Error($"Error parsing JSON response from GitHub: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"An unexpected error occurred during update: {ex.ToString()}");
                }
            });
            return true;
        }
    }
}
