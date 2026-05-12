#if EXILED
using Exiled.API.Interfaces;
using Exiled.Loader;
#endif
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine.Networking;
using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
using static UnityEngine.Networking.UnityWebRequest;
using UncomplicatedCustomItems.API.Extensions;
using MEC;

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
        /// Gets the prefix of the plugin for our APIs
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Gets the UCS APIs endpoint
        /// </summary>
        public string Endpoint { get; } = "https://api.ucserver.it/v3";

        /// <summary>
        /// Gets the CreditTag storage for the plugin, downloaded from our central server
        /// </summary>
        public Dictionary<string, CreditTag> Credits { get; internal set; } = [];

        /// <summary>
        /// Gets the latest <see cref="Version"/> of the plugin, loaded by the UCS cloud
        /// </summary>
        public Version? LatestVersion
        {
            get
            {
                if (field == null)
                    Timing.RunCoroutine(LoadLatestVersionCoroutine());

                return field;
            }

            set;
        }

        /// <summary>
        /// Create a new instance of the HttpManager
        /// </summary>
        /// <param name="prefix"></param>
        public HttpManager(string prefix)
        {
            Prefix = prefix;
            RegisterEvents();
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

        public IEnumerator<float> AddServerOwner(string discordId, Action<HttpStatusCode> onCompleted)
        {
            using UnityWebRequest request = Get($"{Endpoint}/owners/add?discordid={discordId}");
            yield return Timing.WaitUntilDone(request.SendWebRequest());
            onCompleted.Invoke((HttpStatusCode)request.responseCode);
            request.Dispose();
        }

        private IEnumerator<float> LoadLatestVersionCoroutine()
        {
            using UnityWebRequest request = Get($"{Endpoint}/{Prefix}/version?vts=5");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return Timing.WaitUntilDone(request.SendWebRequest());

            if (request.result != Result.Success)
            {
                LogManager.Warn($"Failed to load latest version: {request.error}");
                LatestVersion = new Version();
                yield break;
            }

            string versionString = request.downloadHandler.text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(versionString) && versionString.Contains("."))
            {
                LatestVersion = new Version(versionString);                
            }
            else
                LatestVersion = new Version();

            LogManager.Debug($"Latest version loaded: {LatestVersion}");
        }

        internal IEnumerator<float> LoadCreditTagsCoroutine()
        {
            string[] endpoints =
            [
                "https://api.ucserver.it/credits.json", // Main
                "https://devtagsbackup.thaumiel-servers.workers.dev/" // Backup
            ];

            bool success = false;
            string jsonResponse = string.Empty;

            foreach (string endpoint in endpoints)
            {
                using UnityWebRequest request = Get(endpoint);
                yield return Timing.WaitUntilDone(request.SendWebRequest());

                if (request.result == Result.Success)
                {
                    jsonResponse = request.downloadHandler.text;
                    success = true;
                    LogManager.Info($"Successfully fetched credits data from {endpoint}");
                    break;
                }
                else
                    LogManager.Warn($"Failed to fetch credits data from {endpoint} - {request.error}");
            }

            if (!success)
            {
                LogManager.Error("Failed to fetch credits data from all endpoints in HttpManager::LoadCreditTags()");
                yield break;
            }

            try
            {
                Credits = JsonSerializer.Deserialize<Dictionary<string, CreditTag>>(jsonResponse) ?? [];
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

        public CreditTag GetCreditTag(Player player) =>
            Credits.GetValueSafe(player.UserId);

        public bool TryGetCreditTag(Player player, out CreditTag? output)
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

            if (TryGetCreditTag(player, out var tag) && tag != null)
            {
                if (player.UserGroup != null || player.UserGroup?.Permissions != 0 || !string.IsNullOrWhiteSpace(player.UserGroup.BadgeText))
                {
                    if (tag.Role == player.GroupName && tag.Color == player.GroupColor)
                        return;

                    if (!tag.Override)
                        return;
                }

                if (!string.IsNullOrWhiteSpace(tag.Role) && !string.IsNullOrWhiteSpace(tag.Color))
                {
                    player.GroupName = tag.Role;
                    player.GroupColor = tag.Color;
                }
            }
        }

#nullable enable
        internal void VersionInfo(Action<HttpStatusCode, string?> onCompleted)
        {
            string url = $"https://uciversionmanager.thaumiel-servers.workers.dev/item/{Plugin.Instance.Version.ToString(3)}";
            Timing.RunCoroutine(VersionInfoCoroutine(url, onCompleted));
        }

        private IEnumerator<float> VersionInfoCoroutine(string url, Action<HttpStatusCode, string?> onCompleted)
        {
            using UnityWebRequest request = Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return Timing.WaitUntilDone(request.SendWebRequest());

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
    }
}