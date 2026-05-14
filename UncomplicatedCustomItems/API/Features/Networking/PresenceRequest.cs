using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine.Networking;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class PresenceRequest : APIRequest
    {
        public int FailCount { get; set; } = 0;

        public override string Name => nameof(PresenceRequest);

        public override RequestType Type => RequestType.Post;

        public override string Endpoint => "presence/upload";

        public override Dictionary<string, object> Payload
        {
            get => new()
            {
                ["serverName"] = Server.ServerListName ?? "unknown",
                ["pluginVersion"] = Plugin.Instance?.Version.ToString(3) ?? "unknown",
                ["serverPort"] = Server.Port,
                ["serverIp"] = Server.IpAddress ?? "unknown",
                ["hideIP"] = Plugin.Instance?.Config.HideipOnList.ToString() ?? "false",
                ["scpslVersion"] = GameCore.Version.VersionString ?? "unknown",
                ["showOnList"] = Plugin.Instance?.Config.ShowOnuciList.ToString() ?? "false",
                ["extra"] = $"PlayerCount: {Player.List?.RealList()?.Count() ?? 0}, MaxPlayers: {Server.MaxPlayers}, Idling: {Server.IdleModeActive}, EnabledCreditTags: {Plugin.Instance?.Config.EnableCreditTags}",
            };

            set;
        }

        public override Dictionary<string, string> Headers { get; set; } = new()
        {
            ["Content-Type"] = "application/json"
        };

        public override RequestSettings Settings { get; set; } = new()
        {
            WaitTime = 30f,
            Loop = true,
            Cancel = false,
        };

        public override void OnRequestFailed(UnityWebRequest request)
        {
            FailCount++;
            LogManager.Error($"UCI Presence has failed to send: {FailCount}/5 \n Error: {request.error}");

            if (FailCount >= 5)
            {
                LogManager.Warn($"UCI Presence has failed to send: {FailCount}/5 times, stopping presence updates.");
                Settings.Cancel = true;
            }
        }

        public override void OnRequestCompleted(UnityWebRequest request)
        {
            FailCount = 0;
            base.OnRequestCompleted(request);
        }

        public override void SendRequest(Action<UnityWebRequest>? onComplete = null!)
        {
            if (Settings.Cancel)
                return;

            base.SendRequest();
        }
    }
}
