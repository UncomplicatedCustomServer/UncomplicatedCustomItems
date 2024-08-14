using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Summoned : PlayerCommandBase
    {
        public override string Command => "summoned";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "List every summoned custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (!player.CheckPermission("uci.summoned"))
            {
                response = "Sorry but you don't have the permission to use that command!";
                return false;
            }

            response = "List of every registered custom Items:\n\n Serial | Id | Status |    Name   | Owner";

            foreach (SummonedCustomItem Item in SummonedCustomItem.List)
            {
                string Status = Item.IsPickup ? "Pickup" : " Item ";
                string Owner = (Item.Owner is null) ? "null" : Item.Owner.Nickname;
                response += $"\n   {Item.Serial}    {Item.CustomItem.Id}   {Status}   {Item.CustomItem.Name}   {Owner}";
            }

            return true;
        }
    }
}