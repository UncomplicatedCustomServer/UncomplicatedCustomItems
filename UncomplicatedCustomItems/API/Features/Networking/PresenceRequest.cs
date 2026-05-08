using LabApi.Features.Wrappers;
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

        public override RequestType Type => RequestType.Post;

        public override string Endpoint => "presence/upload";

        public override Dictionary<string, object> Payload { get; set; } = new()
        {
            ["serverName"] = Server.ServerListName,
            ["pluginVersion"] = Plugin.Instance.Version.ToString(3) ?? "unknown",
            ["serverPort"] = Server.Port,
            ["serverIp"] = Server.IpAddress,
            ["hideIP"] = Plugin.Instance.Config.HideipOnList.ToString(),
            ["scpslVersion"] = GameCore.Version.VersionString,
            ["showOnList"] = Plugin.Instance.Config.ShowOnuciList.ToString(),
            ["extra"] = $"PlayerCount: {Player.List.RealList().Count()}, MaxPlayers: {Server.MaxPlayers}, Idling: {Server.IdleModeActive}, EnabledCreditTags: {Plugin.Instance.Config.EnableCreditTags}",
        };

        private readonly RequestSettings _settings = new()
        {
            WaitTime = 30f,
            Loop = true,
            Cancel = false,
        };

        public override RequestSettings Settings => _settings;

        public override void OnRequestFailed(UnityWebRequest request)
        {
            LogManager.Error($"UCI Presence has failed to send: {FailCount}/5 \n Error: {request.error}");
            FailCount++;
        }

        public override void OnRequestCompleted(UnityWebRequest request)
        {
            FailCount = 0;

        }

        public override void SendRequest()
        {
            if (FailCount <= 5)
            {
                base.SendRequest();
                return;
            }

            LogManager.Warn($"UCI Presence has failed to send: {FailCount}/5 times stopping...");
            Settings.Cancel = true;
        }
    }
}
