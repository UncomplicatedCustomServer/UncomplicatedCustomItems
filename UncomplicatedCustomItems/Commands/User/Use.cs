using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.User
{
    public class Use : PlayerCommandBase
    {
        public override string Command => "use";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Use item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (player.CurrentItem is null || !Plugin.API.TryGet(player.CurrentItem.Serial, out var result) || result is not CustomItem customItem)
            {
                response = "You must hold the custom item!";
                return false;
            }

            customItem.Execute();

            response = "Used";
            return true;
        }
    }
}
