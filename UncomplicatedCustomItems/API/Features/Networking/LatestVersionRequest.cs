using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine.Networking;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class LatestVersionRequest : APIRequest
    {
        public class VersionResponse
        {
            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;
        }

        public static Version? LatestVersion { get; set; }

        public override string Endpoint => $"versions/latest";

        public override string Name => nameof(LatestVersionRequest);

        public override RequestType Type => RequestType.Get;

        public override void OnRequestCompleted(UnityWebRequest request)
        {
            base.OnRequestCompleted(request);
            VersionResponse? response = JsonSerializer.Deserialize<VersionResponse>(request.downloadHandler.text);
            if (response == null)
            {
                LatestVersion = new();
                return;
            }

            string version = response.Version;
            if (!string.IsNullOrEmpty(version) && version.Contains("."))
            {
                LatestVersion = new(version);
            }
            else
                LatestVersion = new();
        }
    }
}