﻿using CommandSystem;
using Exiled.API.Features;
using System;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class List : PlayerCommandBase
    {
        public override string Command => "list";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "List every loaded custom item";

        public override bool Execute(ArraySegment<string> arguments, Player player, out string response)
        {
            response = "List of every registered custom Items:\n\n Id |  Type  |   Name";

            foreach (ICustomItem Item in Manager.Items.Values)
            {
                response += $"\n  {Item.Id}   {Item.CustomItemType}    {Item.Name}";
            }

            return true;
        }
    }
}