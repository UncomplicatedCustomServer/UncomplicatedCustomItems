using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Random : Subcommand
    {
        public override string Name { get; } = "random";

        public override string Description { get; } = "Gets a random Item or CustomItem";

        public override string VisibleArgs { get; } = "";

        public override int RequiredArgsCount { get; } = 1;

        public override string RequiredPermission { get; } = "uci.random";

        public override string[] Aliases { get; } = ["ran"];

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string text = string.Empty;
            Player? player = Player.Get(sender);
            if (player == null)
            {
                response = "Player is null!";
                return false;
            }

            switch (arguments.At(0))
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