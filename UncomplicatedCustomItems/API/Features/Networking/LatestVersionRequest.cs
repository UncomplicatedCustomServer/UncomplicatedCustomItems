using System;
using UnityEngine.Networking;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class LatestVersionRequest : APIRequest
    {
        public static Version? LatestVersion { get; set; }

        public override string Endpoint => $"versions/latest@text/plain";

        public override string Name => nameof(LatestVersionRequest);

        public override RequestType Type => RequestType.Get;

        public override void OnRequestCompleted(UnityWebRequest request)
        {
            base.OnRequestCompleted(request);
            string version = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(version) && version.Contains("."))
            {
                LatestVersion = new(version);
            }
            else
                LatestVersion = new();
        }
    }
}