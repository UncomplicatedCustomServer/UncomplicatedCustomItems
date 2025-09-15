using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.HarmonyElements.Patches.CandyPatches;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Give : ISubcommand
    {
        public string Name { get; } = "give";

        public string Description { get; } = "Give a Custom Item to a specific player or to yourself";

        public string VisibleArgs { get; } = "<Item Id> <Player Id/All>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.give";

        public string[] Aliases { get; } = ["g"];

        private static string StripTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, "<.*?>", "");
        }

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            ICustomItem customItem = null;
            if (uint.TryParse(arguments[0], out uint id))
            {
                Utilities.TryGetCustomItem(id, out ICustomItem iCustomItem);
                customItem = iCustomItem;
            }
            else
            {
                string name = StripTags(arguments[0]);
                Utilities.TryGetCustomItemByName(name, out ICustomItem iCustomItem);
                customItem = iCustomItem;
            }

            if (customItem.Item == ItemType.SCP330 && customItem.CustomData is ICandyData candyData)
            {
                TryAddSpecificPatches.LastCustomItemId = customItem.Id;
                TryAddSpecificPatches.CustomItem = customItem;
                TryAddSpecificPatches.LastDesiredCandy = candyData.CandyType;
            }


            if (arguments.Count == 2)
            {
                if (arguments[1].ToLower() == "all")
                {
                    foreach (Player player in Player.ReadyList.Where(p => !p.IsInventoryFull))
                        new SummonedCustomItem(customItem, player);

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
