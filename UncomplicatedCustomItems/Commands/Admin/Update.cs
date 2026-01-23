using CommandSystem;
using System;
using LabApi.Features.Wrappers;
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

            string arg = arguments.Count > 0 ? arguments.At(0) : "false";
            response = $"Attempting to update UncomplicatedCustomBots from version {Plugin.Instance.Version}. Check console for details.";
#if EXILED
			Server.Host?.ReferenceHub.StartCoroutine(Updater.UpdatePluginCoroutine(Plugin.Instance.Version, arg));
#else
            Player.Host?.ReferenceHub.StartCoroutine(Updater.UpdatePluginCoroutine(Plugin.Instance.Version, arg));
#endif
            return true;
        }
    }
}