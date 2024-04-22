using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UncomplicatedCustomItems.API;
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
            if (!player.CheckPermission("uci.use"))
            {
                response = "Sorry but you don't have the permission to use that command!";
                return false;
            }

            if (player.CurrentItem is null || !Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem Item) || Item.CustomItem.CustomItemType != CustomItemType.Item)
            {
                response = "You must hold the custom item!";
                return false;
            }

            Item.HandleEvent(player, ItemEvents.Command);

            response = $"Item {Item.CustomItem.Name} successfully used!";
            return true;
        }
    }
}
