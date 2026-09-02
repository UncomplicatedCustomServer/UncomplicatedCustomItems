using System;
using CommandSystem;

namespace UncomplicatedCustomItems.API.Features
{
    public abstract class Subcommand
    {
        public abstract string Name { get; }

        public abstract string VisibleArgs { get; }

        public virtual int RequiredArgsCount { get; }

        public abstract string Description { get; }

        public virtual string[] Aliases { get; } = [];

        public abstract string RequiredPermission { get; }

        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);
    }
}