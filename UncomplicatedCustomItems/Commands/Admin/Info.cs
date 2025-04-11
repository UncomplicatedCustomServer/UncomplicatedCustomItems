using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Info : ISubcommand
    {
        public string Name { get; } = "info";

        public string Description { get; } = "Get info on a summoned custom item";

        public string VisibleArgs { get; } = "<Item Serial>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.info";

        public string[] Aliases { get; } = ["info"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            if (args.Count == 0)
            {
                response = $"usage: <Item Serial>";
                return false;
            }
            if (!Utilities.TryGetSummonedCustomItem(ushort.Parse(args[0]), out SummonedCustomItem CustomItem))
            {
                response = $"{ushort.Parse(args[0])} is not a custom item serial.";
                return false;
            }
            
            response = $"\nPosition: {CustomItem.Pickup.Position}\nRelative Position Inside Room: {CustomItem.Pickup.Room.transform.InverseTransformPoint(CustomItem.Pickup.Position)}\nRoom: {CustomItem.Pickup.Room}";
            return true;
        }
    }
}
