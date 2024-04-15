using CommandSystem;
using Exiled.API.Features.Pools;
using System;
using System.Linq;
using System.Text;
using UncomplicatedCustomItems.Commands.Admin;
using UncomplicatedCustomItems.Commands.User;

namespace UncomplicatedCustomItems.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Parent : ParentCommand
    {
        public Parent()
        {
            LoadGeneratedCommands();
        }

        public override string Command => "uci";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Parent command";

        private StringBuilder _message;

        ~Parent()
        {
            StringBuilderPool.Pool.Return(_message);
        }

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Read());
            RegisterCommand(new Use());
            RegisterCommand(new Create());

            _message = StringBuilderPool.Pool.Get(AllCommands.Count());

            foreach (var command in AllCommands)
            {
                _message.AppendLine($"{command.Command} - {command.Description}");
            }
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = _message.ToString();

            return true;
        }
    }
}
