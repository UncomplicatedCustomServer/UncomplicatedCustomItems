using System.Collections.Generic;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class AutoLogRequest : APIRequest
    {
        public AutoLogRequest(string log, string runner, string level)
        {
            Payload = new()
            {
                ["plugin_version"] = Plugin.Instance.Version,
                ["log_data"] = log,
                ["runner"] = runner,
                ["level"] = level
            };
        }

        public override string Name => nameof(AutoLogRequest);

        public override string Endpoint => "logs/auto/upload";

        public override RequestType Type => RequestType.Post;

        public override Dictionary<string, string> Headers { get; set; } = new()
        {
            ["Content-Type"] = "application/json"
        };
    }
}