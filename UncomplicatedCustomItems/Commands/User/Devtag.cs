using CommandSystem;
using System;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.Commands.User
{
    [CommandHandler(typeof(ClientCommandHandler))]
    internal class Devtag : ParentCommand
    {
        public Devtag() => LoadGeneratedCommands();

        public override string Command => "ucidevtag";

        public override string[] Aliases { get; } = [];

        public override string Description => "Get your dev tag!";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (sender.LogName is "SERVER CONSOLE" || player is null)
            {
                response = "Can't use this command while not in the game!";
                return false;
            }

            if (!Plugin.Instance.Config.EnableCreditTags)
            {
                response = "Credit tags are disabled!";
                return false;
            }

            Plugin.HttpManager.ApplyCreditTag(player);

            response = string.Empty;
            return true;
        }
    }
}
