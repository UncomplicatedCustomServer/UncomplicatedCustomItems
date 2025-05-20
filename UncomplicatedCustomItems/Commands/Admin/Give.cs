using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Give : ISubcommand
    {
        public string Name { get; } = "give";

        public string Description { get; } = "Give a Custom Item to a specific player or to yourself";

        public string VisibleArgs { get; } = "<Item Id> (Player Id/Name or All)";

        public int RequiredArgsCount { get; } = 1;

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.GivingItems;

        public string[] Aliases { get; } = ["g"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!CustomItem.CustomItems.ContainsKey(uint.Parse(arguments[0])))
            {
                response = $"Sorry but there's no custom item with the Id {uint.Parse(arguments[0])}!";
                return false;
            }

            ICustomItem customItem = CustomItem.CustomItems[uint.Parse(arguments[0])];

            if (arguments.Count == 2)
            {
                if (arguments[1].ToLower() == "all")
                {
                    foreach (Player player in Player.List)
                    {
                        new SummonedCustomItem(customItem, player);
                    }
                    response = $"Successfully gave '{customItem.Name}' to all players!";
                    return true;
                }
                else
                {
                    Player target = Player.Get(int.Parse(arguments[1]));
                    if (target is null)
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
                    new SummonedCustomItem(customItem, target);
                    response = $"Successfully gave '{customItem.Name}' to player {target.Nickname}";
                    return true;
                }
            }
            else
            {
                Player target = Player.Get(sender);
                if (target is null)
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
                new SummonedCustomItem(customItem, target);
                response = $"Successfully gave '{customItem.Name}' to player {target.Nickname}";
                return true;
            }
        }
    }
}
