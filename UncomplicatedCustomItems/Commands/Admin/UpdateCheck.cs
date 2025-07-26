using CommandSystem;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UncomplicatedCustomItems.API.Features.Helper;

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
        public override string[] Aliases { get; } = new string[] { "ucicheckupdate" };
        public override string Description { get; } = "Checks if a new version of UncomplicatedCustomItems is available.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Version version = Plugin.Instance.Version;
            response = $"Currently running version {version}. Checking for updates...";
            
            Task.Run(async () => await UpdateChecker.CheckForUpdatesAsync());
            return true;
        }
    }
}
