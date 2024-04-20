using CommandSystem;
using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Summon : PlayerCommandBase
    {
        public override string Command => "summon";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Summon a custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            if (arguments.Count < 1 || !int.TryParse(arguments.Array[0], out var result))
            {
                response = "You must enter the custom item Id!";
                return false;
            }

            if (!Manager.Items.ContainsKey(uint.Parse(arguments.At(0))))
            {
                response = $"Sorry but there's no custom item with the Id {uint.Parse(arguments.At(0))}!";
                return false;
            }

            Player Target;
            if (arguments.Count == 2) 
            {
                Target = Player.Get(arguments.At(1));
            } else
            {
                Target = player;
            }

            Utilities.Summon(Manager.Items[uint.Parse(arguments.At(0))], Target);

            response = "Summoned!";
            return true;
        }
    }
}
