using CommandSystem;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class List : ISubcommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "Lists every registered Custom Item";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.GivingItems;

        public string[] Aliases { get; } = ["l"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            response = "\nList of every registered custom Item:\n";

            foreach (ICustomItem Item in CustomItem.List.OrderBy(item => item.Id))
                response += $"<size=23><color=#00ff00>✔</color></size> <size=21>[{Item.Id}]</size> <size=19>{Item.CustomItemType} - <color=green>{Item.Name}</color></size>\n";

            if (CustomItem.UnregisteredList.Count > 0)
            {
                response += $"\nList of every unregistered custom Item:\n";

                foreach (ICustomItem Item in CustomItem.UnregisteredList.OrderBy(item => item.Id))
                    response += $"<size=23><color=#ff0000>❌</color></size> <size=21>[{Item.Id}]</size> <size=19>{Item.CustomItemType} - <color=red>{Item.Name}</color></size>\n";
            }

            response += $"\n<color=#00ff00>[✔]</color> {CustomItem.List.Count} Registered CustomItems.\n";

            if (CustomItem.UnregisteredList.Count > 0)
                response += $"<color=#ffff00>[⚠]</color> {CustomItem.UnregisteredList.Count} Unregistered CustomItems.";

            return true;
        }
    }
}
