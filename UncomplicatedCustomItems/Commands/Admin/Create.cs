using CommandSystem;
using Exiled.API.Features;
using System;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Create : PlayerCommand
    {
        public override string Command => "create";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Create custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (arguments.Count == 0 || !int.TryParse(arguments.Array[2], out var result))
            {
                response = "You must enter the custom item id";
                return false;
            }

            if (!Plugin.Instance.Config.CustomItems.TryGetValue(result, out var value))
            {
                response = $"Unknown custom item with {result} id";
                return false;
            }

           value.Create(player).Spawn();

            response = "Created";
            return true;
        }
    }
}
