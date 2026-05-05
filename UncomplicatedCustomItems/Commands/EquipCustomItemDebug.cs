using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands
{
    public class EquipCustomItemDebug : ISubcommand
    {
        public string Name { get; } = "equipcustomitem";

        public string Description { get; } = "";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.equipcustomitem";

        public string[] Aliases { get; } = ["eci"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Plugin.Instance.Config.Debug)
            {
                response = "Debug is disabled.";
                return false;
            }

            Player player = Player.Get(int.Parse(arguments[0]));
            foreach (Item item in player.Items)
            {
                if (Utilities.TryGetSummonedCustomItem(item.Serial, out var customItem) && customItem.CustomItem.Id == uint.Parse(arguments[1]))
                    player.CurrentItem = item;
            }

            response = "Agh";
            return true;
        }
    }
}