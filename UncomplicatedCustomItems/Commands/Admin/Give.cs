using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.HarmonyElements.Patches;
using UncomplicatedCustomItems.API.Features.SpecificData;
using InventorySystem.Items.Usables.Scp330;
using static UncomplicatedCustomItems.API.Extensions.PlayerExtensions;
using UncomplicatedCustomItems.Events;
using MEC;

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

                else if (APICustomItem.CustomItems.TryGetValue(id, out var baseItem))
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

                else if (APICustomItem.CustomItems.Values.Any(c => c.Name == name))
                {
                    customItemobj = APICustomItem.CustomItems.Values.FirstOrDefault(c => c.Name == name);
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
                    if (customItem.Item == ItemType.SCP330 && customItem.CustomData is CandyData candyData)
                    {
                        Player player = arguments.Count == 2 ? Player.Get(int.Parse(arguments[1])) : Player.Get(sender);

                        if (player.Items.Any(i => i.Base is Scp330Bag bag && bag.Candies.Count >= 6))
                        {
                            response = $"{player.DisplayName}'s Candy Bag is full!";
                            return false;
                        }
                        Scp330Bag bag = player.Items.FirstOrDefault(i => i.Base is Scp330Bag)?.Base as Scp330Bag;
                        if (bag != null)
                            PlayerHandler.CandyIdx.Add(((CustomItem)customItem, bag.ItemSerial, bag.Candies.Count() + 1));
                        else
                        {
                            player.GiveCandy(candyData.CandyType, InventorySystem.Items.ItemAddReason.Undefined);
                            Timing.CallDelayed(Timing.WaitForOneFrame, () => PlayerHandler.CandyIdx.Add(((CustomItem)customItem, bag.ItemSerial, bag.Candies.Count())));
                        }
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

                case APICustomItem baseCustomItem:
                    if (baseCustomItem.Item == ItemType.SCP330 && baseCustomItem is CustomCandy customCandy)
                    {
                        Player player = arguments.Count == 2 ? Player.Get(int.Parse(arguments[1])) : Player.Get(sender);

                        if (player.Items.Any(i => i.Base is Scp330Bag bag && bag.Candies.Count >= 6))
                        {
                            response = $"{player.DisplayName}'s Candy Bag is full!";
                            return false;
                        }

                        Scp330Bag bag = player.Items.FirstOrDefault(i => i.Base is Scp330Bag)?.Base as Scp330Bag;
                        if (bag != null)
                            CustomCandy.Candyidx.Add((customCandy, bag.ItemSerial, bag.Candies.Count() + 1));
                        else
                        {
                            player.GiveCandy(customCandy.CandyType, InventorySystem.Items.ItemAddReason.Undefined);
                            Timing.CallDelayed(Timing.WaitForOneFrame, () => CustomCandy.Candyidx.Add((customCandy, bag.ItemSerial, bag.Candies.Count())));
                        }
                    }

                    if (arguments.Count == 2)
                    {
                        if (arguments[1].ToLower() == "all")
                        {
                            foreach (Player player in Player.ReadyList.Where(p => !p.IsInventoryFull))
                                new SummonedAPICustomItem(baseCustomItem, player);

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
                            new SummonedAPICustomItem(baseCustomItem, target);
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
                        new SummonedAPICustomItem(baseCustomItem, target);
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
