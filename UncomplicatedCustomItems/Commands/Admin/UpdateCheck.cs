using CommandSystem;
using System;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Text.Json.Serialization;
using MEC;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class UpdateCheck : ParentCommand
    {
        public class GitHubReleaseInfo
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; }

            [JsonPropertyName("assets")]
            public Updater.GitHubAssetInfo[] Assets { get; set; }
        }
        
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

            response = $"Currently running version {Plugin.Instance.Version}. Checking for updates...";
            Timing.RunCoroutine(Updater.CheckForUpdatesCoroutine());
            return true;
        }
    }
}
