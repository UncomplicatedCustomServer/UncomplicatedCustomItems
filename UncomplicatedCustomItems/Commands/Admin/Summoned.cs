using CommandSystem;
using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Summoned : PlayerCommandBase
    {
        public override string Command => "summoned";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "List every summoned custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            response = "List of every registered custom Items:\n\n Serial | Id | Status |    Name   | Owner";

            foreach (SummonedCustomItem Item in Manager.SummonedItems)
            {
                string Status = Item.IsPickup ? "Pickup" : " Item ";
                string Owner = (Item.Owner is null) ? "null" : Item.Owner.Nickname;
                response += $"\n   {Item.Serial}    {Item.CustomItem.Id}   {Status}   {Item.CustomItem.Name}   {Owner}";
            }

            return true;
        }
    }
}