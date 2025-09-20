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
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

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

            object customItemobj = null;
            if (uint.TryParse(arguments[0], out uint id))
            {
                if (Utilities.TryGetCustomItem(id, out ICustomItem iCustomItem))
                {
                    customItemobj = iCustomItem;
                }

                else if (BaseCustomItem.CustomItems.TryGetValue(id, out var baseItem))
                {
                    customItemobj = baseItem;
                }
            }
            else
            {
                string name = StripTags(arguments[0]);

                if (Utilities.TryGetCustomItemByName(name, out ICustomItem iCustomItem))
                {
                    customItemobj = iCustomItem;
                }

                else if (BaseCustomItem.CustomItems.Values.Any(c => c.Name == name))
                {
                    customItemobj = BaseCustomItem.CustomItems.Values.FirstOrDefault(c => c.Name == name);
                }
            }

            if (customItemobj == null)
            {
                response = $"Custom item '{arguments[0]}' not found!";
                return false;
            }

            switch (customItemobj)
            {
                case ICustomItem customItem:
                    if (customItem.Item == ItemType.SCP330 && customItem.CustomData is ICandyData candyData)
                    {
                        TryAddSpecificPatches.LastCustomItemId = customItem.Id;
                        TryAddSpecificPatches.CustomItemobj = customItem;
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

                case BaseCustomItem baseCustomItem:
                    if (baseCustomItem.Item == ItemType.SCP330 && baseCustomItem is CustomCandy customCandy)
                    {
                        TryAddSpecificPatches.LastCustomItemId = baseCustomItem.Id;
                        TryAddSpecificPatches.CustomItemobj = baseCustomItem;
                        TryAddSpecificPatches.LastDesiredCandy = customCandy.CandyType;
                    }


                    if (arguments.Count == 2)
                    {
                        if (arguments[1].ToLower() == "all")
                        {
                            foreach (Player player in Player.ReadyList.Where(p => !p.IsInventoryFull))
                                new SummonedBaseCustomItem(baseCustomItem, player);

                            response = $"Successfully gave '{baseCustomItem.Name}' to all players!";
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
                            new SummonedBaseCustomItem(baseCustomItem, target);
                            response = $"Successfully gave '{baseCustomItem.Name}' to player {target.Nickname}";
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
                        new SummonedBaseCustomItem(baseCustomItem, target);
                        response = $"Successfully gave '{baseCustomItem.Name}' to player {target.Nickname}";
                        return true;
                    }
                    
                default:
                    response = "Invalid custom item type.";
                    return false;
            }
        }
    }
}
