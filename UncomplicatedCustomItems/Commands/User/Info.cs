using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.Commands.User
{
    [CommandHandler(typeof(ClientCommandHandler))]
    internal class Info : ParentCommand
    {
        public Info() => LoadGeneratedCommands();

        public override string Command => "customiteminfo";

        public override string[] Aliases { get; } = ["info", "cinfo"];

        public override string Description => "Gets the extended discription of a CustomItem";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player is null || sender.LogName is "SERVER CONSOLE")
            {
                response = "Can't use this command while not in the game!";
                return false;
            }

            if (player.CurrentItem is null || !Utilities.TryGetSummonedCustomItem(player.CurrentItem.Serial, out var item))
            {
                response = "You must hold the custom item!";
                return false;
            }

            if (!string.IsNullOrEmpty(item.CustomItem.ExtendedDescription))
            {
                string description = item.CustomItem.ExtendedDescription
                .Replace("%name%", item.CustomItem.Name)
                .Replace("%playername%", item.Owner.DisplayName)
                .Replace("%player%", item.Owner.DisplayName)
                .Replace("%id%", item.CustomItem.Id.ToString()
                .Replace("%serial%", item.Serial.ToString()));

                response = description;
                return true;
            }
            else
            {
                response = $"{item.CustomItem.Name} Doesn't have a extended discription!";
                return false;
            }
        }
    }
}