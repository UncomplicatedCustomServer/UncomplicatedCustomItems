using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine.Networking;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class CreditsRequest : APIRequest
    {
        public class CreditTag
        {
            [JsonPropertyName("SteamID")]
            public string SteamID { get; set; } = string.Empty;

            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("color")]
            public string Color { get; set; } = string.Empty;

            [JsonPropertyName("override")]
            public bool Override { get; set; }

            [JsonPropertyName("job")]
            public bool Job { get; set; }
        }

        public static Dictionary<string, CreditTag> Credits { get; internal set; } = [];

        public static CreditTag GetCreditTag(Player player) =>
            Credits.GetValueSafe(player.UserId);

        public static bool TryGetCreditTag(Player player, out CreditTag? output)
        {
            output = Credits.GetValueSafe(player.UserId);
            if (output != null)
                return true;
                
            output = null;
            return false;
        }

        public static void ApplyCreditTag(Player player)
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

        internal static void Register()
        {
            PlayerEvents.Joined += OnVerified;
        }

        internal static void Unregister()
        {
            PlayerEvents.Joined -= OnVerified;
        }

        private static void OnVerified(PlayerJoinedEventArgs ev) => ApplyCreditTag(ev.Player);

        public override bool UseUCIEndpoint => false;

        public override string Name => nameof(CreditsRequest);

        public override string Endpoint => "credits.json";

        public override string CustomEndpoint => "https://devtagsbackup.thaumiel-servers.workers.dev/";

        public override RequestType Type => RequestType.Get;

        public override void OnRequestCompleted(UnityWebRequest request)
        {
            base.OnRequestCompleted(request);
            try
            {
                string response = request.downloadHandler.text;

                if (string.IsNullOrEmpty(response) || response == "{}")
                {
                    LogManager.Error("Failed to retrieve credits information. \n Retrying with backup endpoint...");
                    UseCustomEndpoint = true;
                    SendRequest();
                    return;
                }

                if (UseCustomEndpoint)
                {
                    Credits = JsonSerializer.Deserialize<List<CreditTag>>(response).ToDictionary(c => c.SteamID, c => c);
                }
                else
                    Credits = JsonSerializer.Deserialize<Dictionary<string, CreditTag>>(response) ?? [];
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to parse credits response: {ex}");
            }
        }

        public override void OnRequestFailed(UnityWebRequest request)
        {
            base.OnRequestFailed(request);
            LogManager.Error($"Failed to retrieve credits information. Error: {request.error} \n Retrying with backup endpoint...");
            UseCustomEndpoint = true;
            SendRequest();
        }
    }
}