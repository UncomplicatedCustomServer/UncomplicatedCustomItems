using Exiled.API.Features;
using System;

namespace UncomplicatedCustomItems.Commands.User
{
    public class Read : PlayerCommandBase
    {
        public override string Command => "read";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Read custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (player.CurrentItem is null || !Plugin.API.TryGet(player.CurrentItem.Serial, out var result))
            {
                response = "You must hold the custom item!";
                return false;
            }

            response = $"{result.Name}: {result.Description}";
            return true;
        }
    }
}
