using CommandSystem;
using Exiled.API.Features.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.Commands
{
    public abstract class ParentCommandBase : ParentCommand
    {
        public ParentCommandBase()
        {
            LoadGeneratedCommands();
        }

        ~ParentCommandBase()
        {
            StringBuilderPool.Pool.Return(_message);
        }

        public abstract PlayerCommandBase[] Children { get; }

        private StringBuilder _message;

        public override void LoadGeneratedCommands()
        {
            _message = StringBuilderPool.Pool.Get(Children.Length);

            foreach (var child in Children)
            {
                _message.AppendLine($"{child.Command} - {child.Description}");
                RegisterCommand(child);
            }
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = _message.ToString();

            return true;
        }
    }
}
