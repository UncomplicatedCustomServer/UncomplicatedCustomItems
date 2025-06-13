using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class Get : ISubcommand
    {
        public string Name { get; } = "get";

        public string Description { get; } = "Get info on a summoned custom item";

        public string VisibleArgs { get; } = "<Item Serial>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "uci.get";

        public string[] Aliases { get; } = ["get"];

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

            response = $"\nData for {CustomItem.CustomItem.Name} - Serial {CustomItem.Serial}:\n[\U0001F4C1] Position: {CustomItem.Pickup.Position}\n[\U0001F4CC] Relative Position Inside Room: {CustomItem.Pickup.Room.LocalPosition(CustomItem.Pickup.Position)}\n[\U0001F4C4] Room: {CustomItem.Pickup.Room.Name}";
            return true;
        }
    }
}