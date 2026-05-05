using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.User
{
    [CommandHandler(typeof(ClientCommandHandler))]
    internal class Info : ParentCommand
    {
        public Info() => LoadGeneratedCommands();

        public override string Command => "customiteminfo";

        public override string[] Aliases { get; } = ["info"];

        public override string Description => "Gets the extended discription of a CustomItem";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player == null || sender.LogName is "SERVER CONSOLE" || sender.LogName.Contains("Dedicated Server"))
            {
                response = "Cannot use this command while not in the game!";
                return false;
            }

            if (player.CurrentItem == null)
            {
                foreach (Item item in player.Items)
                {
                    if (Utilities.TryGetSummonedCustomItem(item.Serial, out _))
                    {
                        response = "You must hold the CustomItem!";
                        return false;
                    }
                }

                response = $"You do not have a CustomItem!";
                return false;
            }

            if (Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out var customItem) && !string.IsNullOrEmpty(customItem.CustomItem.ExtendedDescription))
            {
                string description = customItem.CustomItem.ExtendedDescription
                .Replace("%name%", customItem.CustomItem.Name)
                .Replace("%playername%", customItem.Owner.DisplayName)
                .Replace("%player%", customItem.Owner.DisplayName)
                .Replace("%id%", customItem.CustomItem.Id.ToString()
                .Replace("%serial%", customItem.Serial.ToString()));

                response = description;
                return true;
            }
            else
            {
                response = $"{customItem.CustomItem.Name} Doesn't have a extended description!";
                return false;
            }
        }
    }
}