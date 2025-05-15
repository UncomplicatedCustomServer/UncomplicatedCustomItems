using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Summon : ISubcommand
    {
        public string Name { get; } = "summon";

        public string Description { get; } = "Summon an existing Custom Item";

        public string VisibleArgs { get; } = "<Item Id>";

        public int RequiredArgsCount { get; } = 1;

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.GivingItems;

        public string[] Aliases { get; } = ["spawn", "s"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsRoundStarted)
            {
                response = "<color=red>[⚠]</color> This command cannot be ran if the round isnt started!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = $"No argument(s) found!\nSyntax: .ucr summon <CustomItem Id> (Player Id)";
                return false;
            }

            if (!CustomItem.CustomItems.ContainsKey(uint.Parse(arguments[0])))
            {
                response = $"Sorry but there's no custom item with the Id {uint.Parse(arguments[0])}!";
                return false;
            }

            ICustomItem customItem = CustomItem.CustomItems[uint.Parse(arguments[0])];

            Utilities.SummonCustomItem(customItem);

            response = $"Successfully summoned 1 '{customItem.Name}' to it's spawn point";
            return true;
        }
    }
}
