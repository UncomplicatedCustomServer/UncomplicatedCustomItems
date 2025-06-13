using CommandSystem;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Enums;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Generate : ISubcommand
    {
        public string Name { get; } = "generate";

        public string Description { get; } = "Generate the specified custom item";

        public string VisibleArgs { get; } = "Id, Name, ItemType, CustomItemType, Description";

        public int RequiredArgsCount { get; } = 6;

        public string RequiredPermission { get; } = "uci.gen";

        public string[] Aliases { get; } = ["gen"];
        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 5)
            {
                response = "Usage: <id> <Name> <ItemType> <CustomItemType> <Description>";
                return false;
            }

            if (!uint.TryParse(arguments[0], out uint itemId))
            {
                response = "Invalid item ID.";
                return false;
            }
            
            if (arguments.Count == 5)
            {
                LogManager.Info($"Custom item with ID {itemId} not found. Generating a new one...");

                string itemName = arguments[1];
                
                if (!Enum.TryParse(arguments[2], true, out ItemType itemType))
                {
                    response = $"Invalid ItemType: {arguments[2]}";
                    return false;
                }
                
                if (!Enum.TryParse(arguments[3], true, out CustomItemType customType))
                {
                    response = $"Invalid CustomItemType: {arguments[3]}";
                    return false;
                }

                string Description = arguments[4];
                FileConfig FileConfig = Plugin.Instance.FileConfig;

                FileConfig.GenerateCustomItem(itemId, itemName, itemType, customType, Description);

                response = $"New custom item '{itemName}' (ID: {FileConfig.NewId}) has been created.";
                return true;
            }
            else
            {
                response = $"Item could not be generated.";
                return false;
            }
        }
    }
}