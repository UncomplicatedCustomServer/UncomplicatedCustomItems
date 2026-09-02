using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using System.Linq;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class ToolGun : Subcommand
    {
        public override string Name { get; } = "toolgun";

        public override string Description { get; } = "Get the ToolGun";

        public override string VisibleArgs { get; } = "<Player Id/Name>";

        public override int RequiredArgsCount { get; } = 1;

        public override string RequiredPermission { get; } = "uci.toolgun";

        public override string[] Aliases { get; } = ["tg"];

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            APICustomItem? customItem = APICustomItem.CustomItems.Values.FirstOrDefault(c => c.Name == "ToolGun");
            if (customItem == null)
            {
                response = "ToolGun custom item is not configured!";
                return false;
            }

            if (arguments.Count == 1)
            {
                if (!int.TryParse(arguments.At(0), out int playerId))
                {
                    response = "Invalid player id!";
                    return false;
                }

                Player? target = Player.Get(playerId);
                if (target == null)
                {
                    response = "Player not found!";
                    return false;
                }
                else if (target.Role == PlayerRoles.RoleTypeId.Spectator || target.Role == PlayerRoles.RoleTypeId.Destroyed)
                {
                    response = "Cannot give items to spectators!";
                    return false;
                }
                else if (target.IsInventoryFull)
                {
                    response = $"{target.Nickname} Inventory is full!";
                    return false;
                }

                new SummonedAPICustomItem(customItem, target);
                response = $"Successfully gave '{customItem.Name}' to player {target.Nickname}";
                return true;
            }
            else
            {
                Player? target = Player.Get(sender);
                if (target == null)
                {
                    response = "Player not found!";
                    return false;
                }
                else if (target.Role == PlayerRoles.RoleTypeId.Spectator || target.Role == PlayerRoles.RoleTypeId.Destroyed)
                {
                    response = "Cannot give items to spectators!";
                    return false;
                }
                else if (target.IsInventoryFull)
                {
                    response = $"{target.Nickname} Inventory is full!";
                    return false;
                }
                
                new SummonedAPICustomItem(customItem, target);
                response = $"Successfully gave '{customItem.Name}' to player {target.Nickname}";
                return true;
            }
        }
    }
}