using CommandSystem;
using System;
using Newtonsoft.Json;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class GitHubReleaseInfo
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("assets")]
        public GitHubAssetInfo[] Assets { get; set; }
    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class UpdateCheck : ParentCommand
    {
        public UpdateCheck() => LoadGeneratedCommands();

        public override string Command { get; } = "uciupdatecheck";
        public override string[] Aliases { get; } = ["ucicheckupdate"];
        public override string Description { get; } = "Checks if a new version of UncomplicatedCustomItems is available.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            Version version = Plugin.Instance.Version;
            response = $"Currently running version {version}. Checking for updates...";

            _ = Task.Run(() => Updater.CheckForUpdatesAsync());
            return true;
        }
    }
}
