using CommandSystem;
using Exiled.API.Features;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Give : ISubcommand
    {
        public string Name { get; } = "give";

        public string Description { get; } = "Give a Custom Item to a specific player or to yourself";

        public string VisibleArgs { get; } = "<Item Id> (Player Id/Name)";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.give";

        public string[] Aliases { get; } = ["g"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!CustomItem.CustomItems.ContainsKey(uint.Parse(arguments[0])))
            {
                response = $"Sorry but there's no custom item with the Id {uint.Parse(arguments[0])}!";
                return false;
            }

            ICustomItem customItem = CustomItem.CustomItems[uint.Parse(arguments[0])];

            Player target;

            if (arguments.Count == 2)
                target = Player.Get(arguments[1]);
            else
                target = Player.Get(sender);

            if (target is null)
            {
                response = "Player not found!";
                return false;
            }

            new SummonedCustomItem(customItem, target);

            response = $"Successfully summoned 1 '{customItem.Name}' to player {target.Nickname}";
            return true;
        }
    }
}
