using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Random : ISubcommand
    {
        public string Name { get; } = "random";

        public string Description { get; } = "Gets a random Item or CustomItem";

        public string VisibleArgs { get; } = "";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.random";

        public string[] Aliases { get; } = ["ran"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            string text = string.Empty;
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "Player is null!";
                return false;
            }

            switch (arguments[0])
            {
                case "Item":
                    string item = Enum.GetNames(typeof(ItemType)).RandomItem();
                    player.AddItem((ItemType)Enum.Parse(typeof(ItemType), item));
                    text = $"Gave normal item to {player.DisplayName}";
                    break;
                case "CustomItem":
                    player.GiveCustomItem(CustomItem.List.RandomItem());
                    text = $"Gave CustomItem to {player.DisplayName}";
                    break;
                case "Both":
                    List<string> both = Enum.GetNames(typeof(ItemType)).ToList();
                    CustomItem.List.ForEach(c => both.Add(c.Name));
                    string ranitem = both.RandomItem();

                    if (Utilities.TryGetCustomItemByName(ranitem, out var item1))
                    {
                        player.GiveCustomItem(item1);
                        text = $"Gave CustomItem to {player.DisplayName}";
                    }
                    else if (Enum.TryParse<ItemType>(ranitem, out ItemType item2))
                    {
                        player.AddItem(item2);
                        text = $"Gave normal item to {player.DisplayName}";
                    }

                    break;
            }

            response = text;
            return true;
        }
    }
}