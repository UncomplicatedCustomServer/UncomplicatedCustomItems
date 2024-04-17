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
    public class Parent : ParentCommandBase
    {
        public Parent() : base() { }

        public override string Command => "uci";

        public override string[] Aliases { get; } = new string[0];

        public override string Description => "Parent command";

        public override PlayerCommandBase[] Children { get; } = new PlayerCommandBase[]
        {
            new Read(),
            new Use(),
            new Create()
        };
    }
}
