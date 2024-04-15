using Exiled.API.Features;
using System;

namespace UncomplicatedCustomItems.Commands.User
{
    public class Use : PlayerCommand
    {
        public override string Command => "use";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Use item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (player.CurrentItem is null || !Plugin.API.TryGet(player.CurrentItem.Serial, out var result))
            {
                response = "You must hold the custom item!";
                return false;
            }

            result.Execute();

            response = "Used";
            return true;
        }
    }
}
