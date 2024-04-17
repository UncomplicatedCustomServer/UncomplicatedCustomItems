using CommandSystem;
using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Commands.Enums;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Create : PlayerCommandBase
    {
        public override string Command => "create";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Create custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (arguments.Count < 2 || !int.TryParse(arguments.Array[2], out var result))
            {
                response = "You must enter the custom item id";
                return false;
            }

            if (!Enum.TryParse(arguments.Array[3], true, out ThingType type))
            {
                response = $"Unknown type with {arguments.Array[3]} name";
                return false;
            }

            CustomThing.Create(player, type, result).Spawn();

            response = "Created";
            return true;
        }
    }
}
