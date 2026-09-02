using System;
using CommandSystem;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Summoned : Subcommand
    {
        public override string Name { get; } = "summoned";

        public override string Description { get; } = "Gets every summoned Custom Item";

        public override string VisibleArgs { get; } = "";

        public override int RequiredArgsCount { get; } = 0;

        public override string RequiredPermission { get; } = "uci.summoned";

        public override string[] Aliases { get; } = [];

        public override bool Execute(ArraySegment<string> arguments, ICommandSender player, out string response)
        {
            response = "List of every summoned Custom Item:\n\n Serial | Id | Status |    Name   | Owner";

            foreach (SummonedCustomItem Item in SummonedCustomItem.List)
            {
                string Status = Item.IsPickup ? "Pickup" : " Item ";
                string Owner = (Item.Owner == null) ? "null" : Item.Owner.Nickname;
                response += $"\n   {Item.Serial}    {Item.CustomItem.Id}   {Status}   {Item.CustomItem.Name}   {Owner}";
            }
            
            foreach (SummonedAPICustomItem item in SummonedAPICustomItem.List)
            {
                string Status = item.IsPickup ? "Pickup" : " Item ";
                string Owner = (item.Owner == null) ? "null" : item.Owner.Nickname;
                response += $"\n   {item.Serial}    {item.CustomItem?.Id}   {Status}   {item.CustomItem?.Name}   {Owner}";
            }

            return true;
        }
    }
}