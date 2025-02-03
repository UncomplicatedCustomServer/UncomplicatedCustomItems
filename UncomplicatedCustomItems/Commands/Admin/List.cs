using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    internal class List : ISubcommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "List every registered Custom Item";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.list";

        public string[] Aliases { get; } = ["l"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            response = "List of every registered custom Items:\n";

            foreach (ICustomItem Item in CustomItem.List)
                response += $"\n({Item.Id}) {Item.CustomItemType} - {Item.Name}";

            return true;
        }
    }
}
