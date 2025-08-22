using CommandSystem;
using System;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Update : ParentCommand
    {
        public Update() => LoadGeneratedCommands();

        public override string Command { get; } = "uciupdate";
        public override string[] Aliases { get; } = ["uciselfupdate"];
        public override string Description { get; } = "Downloads and installs the latest version of UncomplicatedCustomItems, then restarts the server round.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            Version version = Plugin.Instance.Version;
            response = $"Attempting to update UncomplicatedCustomBots from version {version}. Check console for details.";
            _ = Task.Run(() => Updater.UpdatePluginAsync(version, arguments.FirstOrDefault()));
            return true;
        }
    }
}