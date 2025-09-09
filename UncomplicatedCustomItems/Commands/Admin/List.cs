using CommandSystem;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class List : ISubcommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "Lists every registered Custom Item";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string[] RequiredPermission { get; } = ["uci.list"];

        public string[] Aliases { get; } = ["l"];

        readonly StringBuilder sb = new();

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            sb.AppendLine("List of every registered CustomItem & CustomAction:");

            sb.AppendLine("CustomItems:");
            foreach (ICustomItem Item in CustomItem.List.OrderBy(item => item.Id))
                sb.AppendLine($"<size=23><color=#00ff00>✔</color></size> <size=21>[{Item.Id}]</size> <size=19>{Item.CustomItemType} - <color=green>{Item.Name}</color></size>");

            if (CustomItem.UnregisteredList.Count > 0)
            {
                sb.AppendLine($"List of every unregistered custom Item:");

                foreach (ICustomItem item in CustomItem.UnregisteredList.OrderBy(item => item.Id))
                    sb.AppendLine($"<size=23><color=#ff0000>❌</color></size> <size=21>[{item.Id}]</size> <size=19>{item.CustomItemType} - <color=red>{item.Name}</color></size>");
            }

            sb.AppendLine($"<color=#00ff00>[✔]</color> {CustomItem.List.Count} Registered CustomItems.");

            if (CustomItem.UnregisteredList.Count > 0)
                sb.AppendLine($"<color=#ffff00>[⚠]</color> {CustomItem.UnregisteredList.Count} Unregistered CustomItems.");

            sb.AppendLine("CustomActions:");
            foreach (ICustomAction action in CustomAction.List.OrderBy(action => action.Id))
                sb.AppendLine($"<size=23><color=#00ff00>✔</color></size> <size=21>[{action.Id}]</size> - <size=19><color=green>{action.Name}</color></size>");

            if (CustomAction.UnregisteredList.Count > 0)
            {
                sb.AppendLine($"List of every unregistered CustomAction:");

                foreach (ICustomAction action in CustomAction.UnregisteredList.OrderBy(action => action.Id))
                    sb.AppendLine($"<size=23><color=#ff0000>❌</color></size> <size=21>[{action.Id}]</size> - <size=19><color=red>{action.Name}</color></size>");
            }

            sb.AppendLine($"<color=#00ff00>[✔]</color> {CustomAction.List.Count} Registered CustomActions.");

            if (CustomAction.UnregisteredList.Count > 0)
                sb.AppendLine($"<color=#ffff00>[⚠]</color> {CustomAction.UnregisteredList.Count} Unregistered CustomActions.");

            response = sb.ToString();
            return true;
        }
    }
}
