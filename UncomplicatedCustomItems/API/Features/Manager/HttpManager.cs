#if EXILED
using Exiled.API.Interfaces;
using Exiled.Loader;
#endif
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Misc;
using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityEngine.Networking;
using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedCustomItems.API.Features.Helper
{
#pragma warning disable IDE1006
    internal class HttpManager
    {
        public class CreditTag
        {
            public CreditTag(string role, string color, bool overrideStr)
            {
                Role = role;
                Color = color;
                Override = overrideStr;
            }

            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("color")]
            public string Color { get; set; } = string.Empty;

            [JsonPropertyName("override")]
            public bool Override { get; set; }

            [JsonPropertyName("job")]
            public bool Job { get; set; }

            public override string ToString() => $"Text: {Role} Color: {Color} Override: {Override}";
        }

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
        /// Gets the CreditTag storage for the plugin, downloaded from our central server
        /// </summary>
        public Dictionary<string, CreditTag> Credits { get; internal set; } = [];

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

        public HttpStatusCode AddServerOwner(string discordId) => HttpGetRequest($"{Endpoint}/owners/add?discordid={discordId}")?.StatusCode ?? HttpStatusCode.InternalServerError;

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
            Player.Host.ReferenceHub.StartCoroutine(LoadCreditTagsCoroutine());
        }

        private IEnumerator LoadCreditTagsCoroutine()
        {
            using UnityWebRequest request = UnityWebRequest.Get("https://api.ucserver.it/credits.json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                LogManager.Error($"Failed to fetch credits data in HttpManager::LoadCreditTags() - {request.error}");
                yield break;
            }

            try
            {
                string jsonResponse = request.downloadHandler.text;
                Dictionary<string, Dictionary<string, JsonElement>> Data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(jsonResponse);

                foreach (KeyValuePair<string, Dictionary<string, JsonElement>> kvp in Data.Where(kvp => kvp.Value is not null && kvp.Value.ContainsKey("role") && kvp.Value.ContainsKey("color") && kvp.Value.ContainsKey("override") && kvp.Value.ContainsKey("job")))
                {
                    string role = kvp.Value["role"].GetString();
                    string color = kvp.Value["color"].GetString();
                    bool overrideStr = kvp.Value["override"].ValueKind switch
                    {
                        JsonValueKind.String => bool.Parse(kvp.Value["override"].GetString() ?? string.Empty),
                        JsonValueKind.True => true,
                        _ => false
                    };

                    Credits.Add(kvp.Key, new(role, color, overrideStr));
                }
            }
            catch (JsonException je)
            {
                LogManager.Error($"Failed to parse JSON in HttpManager::LoadCreditTags() - {je.GetType().FullName}: {je.Message}\n{je.StackTrace}");
            }
            catch (Exception e)
            {
                LogManager.Error($"Failed to act HttpManager::LoadCreditTags() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}\n{e.HResult}");
            }
        }

        public CreditTag GetCreditTag(Player player)
        {
            return Credits.GetValueSafe(player.UserId);
        }

        public bool TryGetCreditTag(Player player, out CreditTag output)
        {
            output = Credits.GetValueSafe(player.UserId);
            if (output != null)
                return true;
                
            output = null;
            return false;
        }

        public void ApplyCreditTag(Player player)
        {
            if (!Plugin.Instance.Config.EnableCreditTags)
                return;

            CreditTag tag = GetCreditTag(player);
            if (player.UserGroup != null || player.UserGroup.Permissions != 0 || !string.IsNullOrWhiteSpace(player.UserGroup.BadgeText))
            {
                if (tag?.Role == player.GroupName && tag?.Color == player.GroupColor)
                    return;

                if ((bool)!tag?.Override)
                    return;
            }

            if (!string.IsNullOrWhiteSpace(tag.Role) && !string.IsNullOrWhiteSpace(tag.Color))
            {
                player.GroupName = tag.Role;
                player.GroupColor = tag.Color;
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
            HttpResponseMessage message = await HttpClient.GetAsync($"https://uciversionmanager.thaumiel-servers.workers.dev/item/{Plugin.Instance.Version.ToString(3)}");

            if (!message.IsSuccessStatusCode)
                return new(message.StatusCode, null);

            return new(message.StatusCode, await message.Content.ReadAsStringAsync());
        }
    }
}