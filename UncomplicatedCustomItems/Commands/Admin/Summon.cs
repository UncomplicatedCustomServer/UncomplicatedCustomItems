using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;


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
            if (!player.CheckPermission("uci.summon"))
            {
                response = "Sorry but you don't have the permission to use that command!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = $"No argument(s) found!\nSyntax: .uci summon <CustomItem Id> (Player Id)";
                return false;
            }

            if (!Utilities.Items.ContainsKey(uint.Parse(arguments.At(0))))
            {
                response = $"Sorry but there's no custom item with the Id {uint.Parse(arguments.At(0))}!";
                return false;
            }

            Player Target = player;
            if (arguments.Count == 2) 
            {
                Target = Player.Get(arguments.At(1));
            }

            SummonedCustomItem Item = SummonedCustomItem.Summon(Utilities.Items[uint.Parse(arguments.At(0))], Target);

            response = $"Successfully summoned 1 '{Item.CustomItem.Name}' to {Target.Nickname}";
            return true;
        }
    }
}
