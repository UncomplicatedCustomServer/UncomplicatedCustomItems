#if EXILED
using Exiled.Permissions.Extensions;
#endif
using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.Commands.Admin;
using UncomplicatedCustomItems.API.Features.Manager;
using Random = UncomplicatedCustomItems.Commands.Admin.Random;
using UncomplicatedCustomItems.API.Features;
using LabApi.Features.Permissions;

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
            Subcommands.Add(new Random());
            Subcommands.Add(new Info());
            Subcommands.Add(new Get());
            Subcommands.Add(new ToolGun());
            Subcommands.Add(new Errors());
            Subcommands.Add(new Owner());
            Subcommands.Add(new VersionInfo());
            Subcommands.Add(new CustomModuleInfo());
            Subcommands.Add(new Backup());
            
            Subcommands.Add(new EquipCustomItemDebug());
        }

        internal static List<Subcommand> Subcommands { get; } = [];

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (arguments.Count == 0)
                {
                    response = $"UncomplicatedCustomItems v{Plugin.Instance.Version} by FoxWorn3365, SpGerg & Mr. Baguetter\n\n<size=35>Available commands:</size>";
                    foreach (Subcommand command in Subcommands)
                        response += $"\n- uci {command.Name}{(command.VisibleArgs != string.Empty ? $" {command.VisibleArgs}" : "")} - {command.Description}";

                    return true;
                }

                Subcommand cmd = Subcommands.FirstOrDefault(cmd => cmd.Name == arguments.At(0));

                cmd ??= Subcommands.FirstOrDefault(cmd => cmd.Aliases.Contains(arguments.At(0)));

                if (cmd is null)
                {
                    response = "Command not found!";
                    return false;
                }
#if EXILED
                if (!sender.CheckPermission(cmd.RequiredPermission))
#else
                if (!sender.HasPermissions(cmd.RequiredPermission))
#endif
                {
                    response = "You don't have permission to access that command! \n Required permission: {cmd.RequiredPermission}";
                    return false;
                }

                if (arguments.Count < cmd.RequiredArgsCount)
                {
                    response = $"Wrong usage!\nCorrect usage: uci {cmd.Name} {cmd.VisibleArgs}";
                    return false;
                }

                ArraySegment<string> args = arguments.Count > 1 ? new ArraySegment<string>(arguments.Array!, arguments.Offset + 1, arguments.Count - 1) : default;

                return cmd.Execute(args, sender, out response);
            }
            catch (Exception ex)
            {
                Subcommand cmd = Subcommands.FirstOrDefault(cmd => cmd.Name == arguments.At(0));
                cmd ??= Subcommands.FirstOrDefault(cmd => cmd.Aliases.Contains(arguments.At(0)));

                LogManager.Error($"Error when running command {cmd.Name} \n\n {ex.Message} \n\n {ex.StackTrace}");
                response = $"Error when running command {cmd.Name} \n\n {ex.Message} \n\n {ex.StackTrace}";
                return false;
            }
        }
    }
}