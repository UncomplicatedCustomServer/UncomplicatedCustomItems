using CommandSystem;
using System;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Generate : Subcommand
    {
        public override string Name { get; } = "generate";

        public override string Description { get; } = "Generate the specified custom item";

        public override string VisibleArgs { get; } = "Id, Name, ItemType, CustomItemType, Description";

        public override int RequiredArgsCount { get; } = 6;

        public override string RequiredPermission { get; } = "uci.generate";

        public override string[] Aliases { get; } = ["gen"];
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 5)
            {
                response = "Usage: <id> <Name> <ItemType> <CustomItemType> <Description>";
                return false;
            }

            if (!uint.TryParse(arguments.At(0), out uint itemId))
            {
                response = "Invalid item ID.";
                return false;
            }
            
            if (arguments.Count == 5)
            {
                LogManager.Info($"Custom item with ID {itemId} not found. Generating a new one...");

                string itemName = arguments.At(1);
                
                if (!Enum.TryParse(arguments.At(2), true, out ItemType itemType))
                {
                    response = $"Invalid ItemType: {arguments.At(2)}";
                    return false;
                }
                
                if (!Enum.TryParse(arguments.At(3), true, out CustomItemType customType))
                {
                    response = $"Invalid CustomItemType: {arguments.At(3)}";
                    return false;
                }

                string Description = arguments.At(4);
                FileConfig FileConfig = Plugin.Instance.FileConfig;
                YAMLCustomItem item = FileConfig.GenerateCustomItem(itemId, itemName, itemType, customType, Description);

                response = $"New custom item '{itemName}' (ID: {item.Id}) has been created.";
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