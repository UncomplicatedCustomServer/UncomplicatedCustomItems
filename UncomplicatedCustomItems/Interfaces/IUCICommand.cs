using CommandSystem;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Interfaces
{
    internal interface IUCICommand
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string RequiredPermission { get; }

        public abstract bool Executor(List<string> arguments, ICommandSender sender, out string response);
    }
}
