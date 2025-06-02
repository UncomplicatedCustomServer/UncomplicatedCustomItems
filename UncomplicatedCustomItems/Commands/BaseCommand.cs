using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.Commands.Admin;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BaseCommand : ParentCommand
    {
        public BaseCommand() => LoadGeneratedCommands();

        public override string Command => "uci";

        public override string Description => "Manage the UncomplicatedCustomItems plugin features";

        public override string[] Aliases => [];

        public override void LoadGeneratedCommands()
        {
            Subcommands.Add(new List());
            Subcommands.Add(new Give());
            Subcommands.Add(new Summon());
            Subcommands.Add(new Summoned());
            Subcommands.Add(new Generate());
            Subcommands.Add(new Reload());
            Subcommands.Add(new Info());
            Subcommands.Add(new Get());
        }

        private List<ISubcommand> Subcommands { get; } = [];

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = $"UncomplicatedCustomItems v{Plugin.Instance.Version} by FoxWorn3365, SpGerg & Mr. Baguetter\n\nAvailable commands:";
                foreach (ISubcommand command in Subcommands)
                    response += $"- uci {command.Name}{(command.VisibleArgs != string.Empty ? $" {command.VisibleArgs}" : "")} - {command.Description}";

                return true;
            }

            ISubcommand cmd = Subcommands.FirstOrDefault(cmd => cmd.Name == arguments.At(0));

            cmd ??= Subcommands.FirstOrDefault(cmd => cmd.Aliases.Contains(arguments.At(0)));

            if (cmd is null)
            {
                response = "Command not found!";
                return false;
            }

            if (cmd.RequiredPermission != string.Empty && !sender.CheckPermission(cmd.RequiredPermission))
            {
                response = "You don't have permission to access that command!";
                return false;
            }

            if (arguments.Count < cmd.RequiredArgsCount)
            {
                response = $"Wrong usage!\nCorrect usage: uci {cmd.Name} {cmd.VisibleArgs}";
                return false;
            }

            List<string> args = [.. arguments];
            args.RemoveAt(0);

            return cmd.Execute(args, sender, out response);
        }
    }
}
