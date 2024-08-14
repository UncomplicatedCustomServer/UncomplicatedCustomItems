using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class List : PlayerCommandBase
    {
        public override string Command => "list";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "List every loaded custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (!player.CheckPermission("uci.list"))
            {
                response = "Sorry but you don't have the permission to use that command!";
                return false;
            }

            response = "List of every registered custom Items:\n\n Id |  Type  |   Name";

            foreach (ICustomItem Item in CustomItem.List)
            {
                response += $"\n  {Item.Id}   {Item.CustomItemType}    {Item.Name}";
            }

            return true;
        }
    }
}