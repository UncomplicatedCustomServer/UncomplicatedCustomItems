using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UnityEngine;

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
            if (Utilities.TryGetSummonedCustomItem(ushort.Parse(args[0]), out SummonedCustomItem? customItem) && customItem != null)
            {
                response = $"\nData for {customItem.CustomItem.Name} - Serial {customItem.Serial}:\n[\U0001F4C1] Position: {customItem.Pickup?.Position}\n[\U0001F4CC] Relative Position Inside Room: {customItem.Pickup?.Room?.LocalPosition(customItem.Pickup.Position)}\n[\U0001F4C4] Room: {customItem.Pickup?.Room?.Name}";
                return true;
            }
            if (SummonedAPICustomItem.TryGet(ushort.Parse(args[0]), out var baseCustomItem) && baseCustomItem != null)
            {
                response = $"\nData for {baseCustomItem.CustomItem?.Name} - Serial {baseCustomItem.Serial}:\n[\U0001F4C1] Position: {baseCustomItem.Pickup?.Position}\n[\U0001F4CC] Relative Position Inside Room: {baseCustomItem.Pickup?.Room?.LocalPosition(baseCustomItem.Pickup?.Position ?? Vector3.zero)}\n[\U0001F4C4] Room: {baseCustomItem.Pickup?.Room?.Name}";
                return true;
            }

            response = $"{ushort.Parse(args[0])} is not a custom item serial.";
            return false;
        }
    }
}