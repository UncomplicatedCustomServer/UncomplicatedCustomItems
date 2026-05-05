using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class ToolGun : ISubcommand
    {
        public string Name { get; } = "toolgun";

        public string Description { get; } = "Get the ToolGun";

        public string VisibleArgs { get; } = "<Player Id/Name>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.toolgun";

        public string[] Aliases { get; } = ["tg"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            APICustomItem customItem = APICustomItem.CustomItems.Values.Where(c => c.Name == "ToolGun").FirstOrDefault();

            if (arguments.Count == 1)
            {
                Player target = Player.Get(int.Parse(arguments[0]));
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
                Player target = Player.Get(sender);
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
