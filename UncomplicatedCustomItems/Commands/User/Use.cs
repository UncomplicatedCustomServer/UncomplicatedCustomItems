using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Text.RegularExpressions;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Commands.User
{
    [CommandHandler(typeof(ClientCommandHandler))]
    internal class Use : ParentCommand
    {
        public Use() => LoadGeneratedCommands();

        public override string Command => "use";

        public override string[] Aliases { get; } = [];

        public override string Description => "Use the current item";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("uci.use"))
            {
                response = "Sorry but you don't have the permission to use that command!";
                return false;
            }

            Player player = Player.Get(sender);

            if (player is null)
            {
                response = "Can't use this command while not in the game!";
                return false;
            }

            if (player.CurrentItem is null || !Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem Item) || Item.CustomItem.CustomItemType != CustomItemType.Item)
            {
                response = "You must hold the custom item!";
                return false;
            }

            // Ok now we have to check if the custom item command contains any & (= args)
            IItemData Data = Item.CustomItem.CustomData as IItemData;
            if (Data.Command is not null && Data.Command.Contains("#"))
            {
                // yes, the command requires args
                // Let's see how many
                int count = Regex.Matches(Data.Command, "#").Count;
                if (arguments.Count < count)
                {
                    // Error: too few arguments!
                    response = $"Sorry but this command requires {count} arguments, {arguments.Count} found.";
                    return false;
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        int IndexToReplace = Data.Command.IndexOf('#');
                        if (IndexToReplace != -1) // Verifica se è stato trovato un indice valido
                        {
                            Data.Command = Data.Command.Substring(0, IndexToReplace) + arguments.At(i) + Data.Command.Substring(IndexToReplace + 1);
                        }
                    }
                }
                Item.CustomItem.CustomData = Data;
            }

            Item.HandleEvent(player, ItemEvents.Command);

            response = $"Item {Item.CustomItem.Name} successfully used!";
            return true;
        }
    }
}
