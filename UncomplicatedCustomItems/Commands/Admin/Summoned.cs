using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Summoned : ISubcommand
    {
        public string Name { get; } = "summoned";

        public string Description { get; } = "Gets every summoned Custom Item";

        public string VisibleArgs { get; } = "";

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.summoned";

        public string[] Aliases { get; } = [];

        public bool Execute(List<string> arguments, ICommandSender player, out string response)
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
                response += $"\n   {item.Serial}    {item.CustomItem.Id}   {Status}   {item.CustomItem.Name}   {Owner}";
            }

            return true;
        }
    }
}