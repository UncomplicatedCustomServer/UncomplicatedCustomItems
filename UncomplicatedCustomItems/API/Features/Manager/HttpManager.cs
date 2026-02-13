#if EXILED
using Exiled.API.Interfaces;
using Exiled.Loader;
#endif
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using LabApi.Features.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine.Networking;
using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
using static UnityEngine.Networking.UnityWebRequest;

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
            LoadCreditTags();
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

        public IEnumerator AddServerOwner(string discordId, Action<HttpStatusCode> onCompleted)
        {
            using UnityWebRequest request = Get($"{Endpoint}/owners/add?discordid={discordId}");
            yield return request.SendWebRequest();
            onCompleted.Invoke((HttpStatusCode)request.responseCode);
            request.Dispose();
        }

        public void LoadLatestVersion()
        {
            Player.Host?.ReferenceHub.StartCoroutine(LoadLatestVersionCoroutine());
        }

        private IEnumerator LoadLatestVersionCoroutine()
        {
            string url = $"{Endpoint}/{Prefix}/version?vts=5";

            using UnityWebRequest request = Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != Result.Success)
            {
                LogManager.Warn($"Failed to load latest version: {request.error}");
                _latestVersion = new Version();
                yield break;
            }

            string versionString = request.downloadHandler.text?.Trim();
            if (!string.IsNullOrEmpty(versionString) && versionString.Contains("."))
            {
                _latestVersion = new Version(versionString);                
            }
            else
                _latestVersion = new Version();

            LogManager.Debug($"Latest version loaded: {_latestVersion}");
        }

        public void LoadCreditTags()
        {
            Player.Host.ReferenceHub.StartCoroutine(LoadCreditTagsCoroutine());
        }

        private IEnumerator LoadCreditTagsCoroutine()
        {
            using UnityWebRequest request = Get("https://api.ucserver.it/credits.json");
            yield return request.SendWebRequest();

            if (request.result != Result.Success)
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
                    request.Dispose();
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

#nullable enable
        internal void VersionInfo(Action<HttpStatusCode, string?> onCompleted)
        {
            string url = $"https://uciversionmanager.thaumiel-servers.workers.dev/item/{Plugin.Instance.Version.ToString(3)}";
            Player.Host?.ReferenceHub.StartCoroutine(VersionInfoCoroutine(url, onCompleted));
        }

        private IEnumerator VersionInfoCoroutine(string url, Action<HttpStatusCode, string?> onCompleted)
        {
            using UnityWebRequest request = Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            HttpStatusCode status = (HttpStatusCode)request.responseCode;
            if (request.result != Result.Success)
            {
                LogManager.Warn($"[VersionInfo] Request failed: {request.error}");
                onCompleted?.Invoke(status, null);
                request.Dispose();
                yield break;
            }

            onCompleted?.Invoke(status, request.downloadHandler.text);
            request.Dispose();
        }


        public void ShareLogs(string data, Action<HttpStatusCode, string> onCompleted) =>
            Player.Host?.ReferenceHub.StartCoroutine(UploadLogs(data, onCompleted));

        private IEnumerator UploadLogs(string data, Action<HttpStatusCode, string> onCompleted)
        {
#if EXILED
            UnityWebRequest request = new($"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={Loader.Version}&using_labapi=false&plugin_version={Plugin.Instance.Version.ToString(3)}&hash={VersionManager.HashFile(Plugin.Instance.Assembly.GetPath())}");
#else
            UnityWebRequest request = new($"{Endpoint}/{Prefix}/error?port={Server.Port}&exiled_version={LabApiProperties.CompiledVersion}&using_labapi=true&plugin_version={Plugin.Instance.Version.ToString(3)}&hash={VersionManager.HashFile(Plugin.Instance.FilePath)}");
#endif

            byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "text/plain");
            request.method = kHttpVerbPUT;
            yield return request.SendWebRequest();
            HttpStatusCode status = (HttpStatusCode)request.responseCode;
            LogManager.Debug($"Uploaded data to endpoint. Status: {status}");
            onCompleted?.Invoke(status, request.downloadHandler.text);

            request.Dispose();
        }
    }
}