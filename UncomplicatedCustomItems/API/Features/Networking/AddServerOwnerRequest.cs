using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class AddServerOwnerRequest : APIRequest
    {
        public AddServerOwnerRequest(Player player, string discordId)
        {
            Payload = new()
            {
                ["user_id"] = player.UserId,
                ["discord_id"] = discordId
            };
        }

        public override string Endpoint => $"v3/owners";

        public override bool UseUCIEndpoint => false;

        public override string Name => nameof(AddServerOwnerRequest);

        public override RequestType Type => RequestType.Post;

        public override Dictionary<string, string> Headers { get; set; } = new()
        {
            ["Content-Type"] = "application/json"
        };
    }
}