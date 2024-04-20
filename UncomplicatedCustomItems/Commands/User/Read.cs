using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.User
{
    public class Read : PlayerCommandBase
    {
        public override string Command => "read";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Read the custom item's content";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (player.CurrentItem is null || !Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem Item))
            {
                response = "You must hold the custom item!";
                return false;
            }

            response = $"{Item.CustomItem.Name}: {Item.CustomItem.Description}";
            return true;
        }
    }
}
