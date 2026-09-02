namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class VersionInfoRequest : APIRequest
    {
        public override string CustomEndpoint => $"https://uciversionmanager.thaumiel-servers.workers.dev/item/{Plugin.Instance.Version.ToString(3)}";

        public override string Name => nameof(VersionInfoRequest);

        public override RequestType Type => RequestType.Get;

        public override bool UseCustomEndpoint { get; set; } = true;
    }
}