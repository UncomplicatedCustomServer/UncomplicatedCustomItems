#if EXILED
using Exiled.API.Interfaces;
using Exiled.Loader;
#endif
using CentralAuth;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Misc;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Struct;

using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.Helper
{
#pragma warning disable IDE1006

    internal class HttpManager
    {
        private int _presenceIntervalSeconds = 60;
        private bool _presenceRunning = false;
        private int _presenceFailureCount = 0;

        /// <summary>
        /// Gets the <see cref="CoroutineHandle"/> of the presence coroutine.
        /// </summary>
        public CoroutineHandle PresenceCoroutine { get; internal set; }

        /// <summary>
        /// Gets if the feature can be activated - missing library
        /// </summary>
        public bool IsAllowed { get; internal set; } = true;

        /// <summary>
        /// Gets the prefix of the plugin for our APIs
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Gets the <see cref="HttpClient"/> public instance
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the UCS APIs endpoint
        /// </summary>
        public string Endpoint { get; } = "https://api.ucserver.it/v2";

        /// <summary>
        /// Gets the UCI API endpoint
        /// </summary>
        public string UCIAPIEndpoint { get; } = "https://ucipluginapi.thaumielscpsl.site";

        /// <summary>
        /// Gets the CreditTag storage for the plugin, downloaded from our central server
        /// </summary>
        public Dictionary<string, Triplet<string, string, bool>> Credits { get; internal set; } = new();

        /// <summary>
        /// Gets the role of the given player (as steamid@64) inside UCI
        /// </summary>
        public Dictionary<string, string> OrgPlayerRole { get; } = new();

        /// <summary>
        /// Gets the latest <see cref="Version"/> of the plugin, loaded by the UCS cloud
        /// </summary>
        public Version LatestVersion
        {
            get
            {
                if (_latestVersion is null)
                    LoadLatestVersion();
                return _latestVersion;
            }
        }

        private Version _latestVersion { get; set; } = null;

        /// <summary>
        /// Create a new instance of the HttpManager
        /// </summary>
        /// <param name="prefix"></param>
        public HttpManager(string prefix)
        {
            if (!CheckForDependency())
                Timing.CallContinuously(20f, () => LogManager.Error("You don't have the dependency Newtonsoft.Json installed!\nPlease install it AS SOON POSSIBLE!\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV\nError code: 0x406"));

            Prefix = prefix;
            RegisterEvents();
            HttpClient = new();
            Task.Run(LoadCreditTags);
        }

        internal void RegisterEvents()
        {
            PlayerHandler.Joined += OnVerified;
        }

        internal void UnregisterEvents()
        {
            PlayerHandler.Joined -= OnVerified;
        }

        public void OnVerified(PlayerJoinedEventArgs ev) => ApplyCreditTag(ev.Player);
#if EXILED
        private bool CheckForDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "Newtonsoft.Json");
#else
        private bool CheckForDependency() => AssemblyUtils.GetLoadedAssemblies().Any(assembly => assembly.StartsWith("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase));
#endif
        public HttpResponseMessage HttpGetRequest(string url)
        {
            try
            {
                Task<HttpResponseMessage> Response = Task.Run(() => HttpClient.GetAsync(url));

                Response.Wait();

                return Response.Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public HttpResponseMessage HttpPutRequest(string url, string content)
        {
            try
            {
                Task<HttpResponseMessage> Response = Task.Run(() => HttpClient.PutAsync(url, new StringContent(content, Encoding.UTF8, "text/plain")));
                Response.Wait();
                return Response.Result;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public string RetriveString(HttpResponseMessage response)
        {
            if (response is null)
                return string.Empty;

            return RetriveString(response.Content);
        }

        public string RetriveString(HttpContent response)
        {
            if (response is null)
                return string.Empty;

            Task<string> String = Task.Run(response.ReadAsStringAsync);

            String.Wait();

            return String.Result;
        }

        public void LoadLatestVersion()
        {
            string Version = RetriveString(HttpGetRequest($"{Endpoint}/{Prefix}/version?vts=5"));

            if (Version is not null && Version != string.Empty && Version.Contains("."))
                _latestVersion = new(Version);
            else
                _latestVersion = new();
        }

        public void LoadCreditTags()
        {
            Credits = [];
            try
            {
                Dictionary<string, Dictionary<string, string>> Data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(RetriveString(HttpGetRequest("https://api.ucserver.it/credits.json")));

                if (Data is null)
                {
                    LogManager.Warn("Failed to connect to the UCS Central Server to get the credit tags informations!");
                    return;
                }

                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data.Where(kvp => kvp.Value is not null && kvp.Value.ContainsKey("role") && kvp.Value.ContainsKey("color") && kvp.Value.ContainsKey("override")))
                {
                    Credits.Add(kvp.Key, new(kvp.Value["role"], kvp.Value["color"], bool.Parse(kvp.Value["override"])));
                    if (kvp.Value.TryGetValue("job", out string isJob) && isJob is "true")
                        OrgPlayerRole.Add(kvp.Key, isJob);
                }
            }
            catch (Exception e)
            {
                LogManager.Error($"Failed to act HttpManager::LoadCreditTags() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}\n{e.HResult}");
            }
        }

        public Triplet<string, string, bool> GetCreditTag(Player player)
        {
            if (Credits.ContainsKey(player.UserId))
                return Credits[player.UserId];

            return new(null, null, false);
        }

        public void ApplyCreditTag(Player player)
        {
            if (!Plugin.Instance.Config.EnableCreditTags)
                return;

            // Name => Tag.First
            // Color => Tag.Second
            // Override => Tag.Third

            Triplet<string, string, bool> Tag = GetCreditTag(player);

            if (player.UserGroup != null || player.UserGroup.Permissions != 0 || !string.IsNullOrWhiteSpace(player.UserGroup.BadgeText))
            {
                if (Credits.Any(k => k.Value.First == player.GroupName && k.Value.Second == player.GroupColor))
                    return;

                if (!Tag.Third) // Job
                    return; // Do not override
            }

            if (Tag.First is not null && Tag.Second is not null)
            {
                player.GroupName = Tag.First;
                player.GroupColor = Tag.Second;
            }
        }

        public bool IsLatestVersion(out Version latest)
        {
            latest = LatestVersion;
            if (latest.CompareTo(Plugin.Instance.Version) > 0)
                return false;

            return true;

        }

        public bool IsLatestVersion()
        {
            if (LatestVersion.CompareTo(Plugin.Instance.Version) > 0)
                return false;

            return true;
        }

        /// <summary>
        /// Asynchronously shares logs with the server
        /// </summary>
        /// <param name="data">The log data to share</param>
        /// <returns>A tuple containing the status code and response content</returns>
        internal async Task<(HttpStatusCode statusCode, HttpContent content)> ShareLogsAsync(string data)
        {
            try
            {
#if EXILED
                string url = $"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={Loader.Version}&using_labapi=false&plugin_version={Plugin.Instance.Version.ToString(3)}&hash={VersionManager.HashFile(Plugin.Instance.Assembly.GetPath())}";
#else
                string url = $"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={LabApiProperties.CompiledVersion}&using_labapi=true&plugin_version={Plugin.Instance.Version.ToString(3)}&hash={VersionManager.HashFile(Plugin.Instance.FilePath)}";
#endif

                using StringContent content = new(data, Encoding.UTF8, "text/plain");
                using HttpResponseMessage response = await HttpClient.PutAsync(url, content).ConfigureAwait(false);

                HttpContent responseContent = null;
                if (response.Content != null)
                {
                    string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseContent = new StringContent(responseString, Encoding.UTF8, "application/json");
                }

                return (response.StatusCode, responseContent);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal HttpStatusCode ShareLogs(string data, out HttpContent httpContent)
        {
#if EXILED
            HttpResponseMessage Status = HttpPutRequest($"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={Loader.Version}&using_labapi=false&plugin_version={Plugin.Instance.Version.ToString(3)}&hash={VersionManager.HashFile(Plugin.Instance.Assembly.GetPath())}", data);
#else
            HttpResponseMessage Status = HttpPutRequest($"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={LabApiProperties.CompiledVersion}&using_labapi=true&plugin_version={Plugin.Instance.Version.ToString(3)}&hash={VersionManager.HashFile(Plugin.Instance.FilePath)}", data);
#endif
            httpContent = Status.Content;
            return Status.StatusCode;
        }

        public async Task<HttpResponseMessage> HttpPutRequestAsync(string url, string content) => await HttpClient.PutAsync(url, new StringContent(content, Encoding.UTF8, "text/plain"));

#nullable enable
        internal async Task<Tuple<HttpStatusCode, string?>> VersionInfo()
        {
            HttpResponseMessage message = await HttpClient.GetAsync($"{Endpoint.Replace("/v2", "")}/vinfo/info?v={Plugin.Instance.Version.ToString(3)}");

            if (message.StatusCode != HttpStatusCode.OK)
                return new(message.StatusCode, null);

            return new(message.StatusCode, await message.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Start sending presence updates.
        /// </summary>
        public void StartPresence(int intervalSeconds = 60)
        {
            if (!PlayerAuthenticationManager.OnlineMode)
                return;

            LogManager.Debug("Starting UCI API Presence");
            if (string.IsNullOrWhiteSpace(UCIAPIEndpoint))
                throw new ArgumentException("Presence Worker Url required", nameof(UCIAPIEndpoint));

            if (_presenceRunning)
                StopPresence();

            _presenceIntervalSeconds = Math.Max(5, intervalSeconds);
            _presenceRunning = true;
            _presenceFailureCount = 0;

            PresenceCoroutine = Timing.RunCoroutine(PresenceLoop(), Segment.RealtimeUpdate);
        }

        /// <summary>
        /// Stop presence coroutine.
        /// </summary>
        public void StopPresence()
        {
            LogManager.Debug("Stopping UCI API Presence");
            _presenceRunning = false;
            Timing.KillCoroutines(PresenceCoroutine);
        }

        private IEnumerator<float> PresenceLoop()
        {
            while (_presenceRunning)
            {
                try
                {
                    var presenceTask = SendPresenceOnceAsync();

                    _ = TrackPresenceResult(presenceTask);
                }
                catch (Exception ex)
                {
                    LogManager.Error($"HttpManager: error while scheduling presence send: {ex}");
                    _presenceFailureCount++;

                    if (_presenceFailureCount >= 5)
                    {
                        LogManager.Error($"Presence failed {_presenceFailureCount} consecutive times. Stopping presence updates.");
                        _presenceRunning = false;
                        break;
                    }
                }

                yield return Timing.WaitForSeconds(_presenceIntervalSeconds);
            }
        }

        private async Task TrackPresenceResult(Task<bool> presenceTask)
        {
            try
            {
                bool success = await presenceTask;
                
                if (success)
                {
                    _presenceFailureCount = 0;
                }
                else
                {
                    _presenceFailureCount++;
                    LogManager.Warn($"Presence failed ({_presenceFailureCount}/5)");

                    if (_presenceFailureCount >= 5)
                    {
                        LogManager.Error($"Presence failed {_presenceFailureCount} consecutive times. Stopping presence updates.");
                        _presenceRunning = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error tracking presence result: {ex}");
                _presenceFailureCount++;

                if (_presenceFailureCount >= 5)
                {
                    LogManager.Error($"Presence failed {_presenceFailureCount} consecutive times. Stopping presence updates.");
                    _presenceRunning = false;
                }
            }
        }

        internal async Task<bool> SendPresenceOnceAsync()
        {
            if (string.IsNullOrWhiteSpace(UCIAPIEndpoint))
                return false;

            List<string> pluginNames = [];
            bool hasExiled = false;

            try
            {
                Type loaderType = Type.GetType("Exiled.Loader.Loader, Exiled.Loader");
                if (loaderType != null)
                {
                    PropertyInfo pluginsProperty = loaderType.GetProperty("Plugins", BindingFlags.Public | BindingFlags.Static);
                    if (pluginsProperty != null)
                    {
                        if (pluginsProperty.GetValue(null) is IEnumerable plugins)
                        {
                            foreach (object plugin in plugins)
                            {
                                PropertyInfo nameProperty = plugin.GetType().GetProperty("Name");
                                if (nameProperty != null)
                                {
                                    string name = nameProperty.GetValue(plugin) as string;
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        if (name.StartsWith("Exiled", StringComparison.OrdinalIgnoreCase))
                                            hasExiled = true;
                                        else
                                            pluginNames.Add(name);
                                    }
                                }
                            }
                        }
                    }
                }

                LabApi.Loader.PluginLoader.EnabledPlugins.ToList().ForEach(p =>
                {
                    if (!p.Name.StartsWith("Exiled", StringComparison.OrdinalIgnoreCase))
                        pluginNames.Add(p.Name);
                    else
                        hasExiled = true;
                });

                if (hasExiled)
                    pluginNames.Add("Exiled");

                HashSet<string> ucsPlugins = new(StringComparer.OrdinalIgnoreCase)
                {
                    "uncomplicatedcustomitems",
                    "uncomplicatedcustomroles",
                    "uncomplicatedcustomescapezones",
                    "uncomplicatedcustomteams",
                    "uncomplicatedcustombots"
                };

                Dictionary<string, object> payload = new()
                {
                    ["serverName"] = Server.ServerListName,
                    ["pluginVersion"] = Plugin.Instance.Version.ToString(3) ?? "unknown",
                    ["serverPort"] = Server.Port,
                    ["hideIP"] = Plugin.Instance.Config.HideipOnList.ToString(),
                    ["scpslVersion"] = GameCore.Version.VersionString,
                    ["showOnList"] = Plugin.Instance.Config.ShowOnuciList.ToString(),
                    ["exiled"] = hasExiled.ToString().ToLower(),
                    ["extra"] = $"PlayerCount: {Player.List.RealList().Count()}, MaxPlayers: {Server.MaxPlayers}, Idling: {Server.IdleModeActive}, EnabledCreditTags: {Plugin.Instance.Config.EnableCreditTags}",
                    ["plugins"] = pluginNames.Where(ucsPlugins.Contains).ToList()
                };

                if (Plugin.Instance.Config.ShowPluginsOnList)
                    payload["plugins"] = pluginNames;

                string json = JsonConvert.SerializeObject(payload);

                using StringContent content = new(json, Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await HttpClient.PostAsync($"{UCIAPIEndpoint}/connect", content).ConfigureAwait(false);

                string responseText = string.Empty;
                if (response.Content != null)
                    responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    LogManager.Silent($"Presence posted to {UCIAPIEndpoint} - status {(int)response.StatusCode}. Response: {responseText}");
                    return true;
                }
                else
                {
                    LogManager.Warn($"Presence POST failed for {UCIAPIEndpoint} - status {(int)response.StatusCode}. Response: {responseText}");
                    return false;
                }
            }
            catch (HttpRequestException hre)
            {
                LogManager.Warn($"Presence POST HttpRequestException: {hre.Message}");
                return false;
            }
            catch (TaskCanceledException tce)
            {
                LogManager.Warn($"Presence POST canceled/timed out: {tce.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Unexpected error sending presence: {ex.GetType().FullName}: {ex.Message}");
                return false;
            }
        }
    }
}