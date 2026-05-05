using CommandSystem;
using System;
using System.Text.RegularExpressions;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.Enums;
using LabApi.Features.Wrappers;

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
            Player player = Player.Get(sender);

            if (player == null)
            {
                response = "Can't use this command while not in the game!";
                return false;
            }

            if (player.CurrentItem == null || !Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out SummonedCustomItem item) || item.CustomItem.CustomItemType != CustomItemType.Item)
            {
                response = "You must hold the custom item!";
                return false;
            }

            // Ok now we have to check if the custom item command contains any & (= args)
            IItemData itemData = item.CustomItem.CustomData as IItemData;
            foreach (ItemDataList data in itemData.Data)
            {
                if (data.Command != null && data.Command.Contains("#"))
                {
                    // yes, the command requires args
                    // Let's see how many
                    int count = Regex.Matches(data.Command, "#").Count;
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
                            int IndexToReplace = data.Command.IndexOf('#');
                            if (IndexToReplace != -1) // Verifica se è stato trovato un indice valido
                            {
                                data.Command = data.Command.Substring(0, IndexToReplace) + arguments.At(i) + data.Command.Substring(IndexToReplace + 1);
                            }
                        }
                    }
                    item.CustomItem.CustomData = itemData;
                }
            }

            item.HandleEvent(player, ItemEvents.Command, player.CurrentItem.Serial);

            response = $"Item {item.CustomItem.Name} successfully used!";
            return true;
        }
    }
}
