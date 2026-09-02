using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands
{
    public class EquipCustomItemDebug : Subcommand
    {
        public override string Name { get; } = "equipcustomitem";

        public override string Description { get; } = "";

        public override string VisibleArgs { get; } = string.Empty;

        public override int RequiredArgsCount { get; } = 0;

        public override string RequiredPermission { get; } = "uci.equipcustomitem";

        public override string[] Aliases { get; } = ["eci"];

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Plugin.Instance.Config.Debug)
            {
                response = "Debug is disabled.";
                return false;
            }

            Player? player = Player.Get(int.Parse(arguments.At(0)));
            if (player != null)
            {
                foreach (Item item in player.Items)
                {
                    if (Utilities.TryGetSummonedCustomItem(item.Serial, out var customItem) && customItem?.CustomItem.Id == uint.Parse(arguments.At(1)))
                        player.CurrentItem = item;
                }
            }

            response = "Agh";
            return true;
        }
    }
}