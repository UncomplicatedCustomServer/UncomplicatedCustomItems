using CommandSystem;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Interfaces
{
    internal interface ISubcommand
    {
        public string Name { get; }

        public string VisibleArgs { get; }

        public int RequiredArgsCount { get; }

        public string Description { get; }

        public string[] Aliases { get; }

        public PlayerPermissions RequiredPermission { get; }

        public bool Execute(List<string> arguments, ICommandSender sender, out string response);
    }
}
