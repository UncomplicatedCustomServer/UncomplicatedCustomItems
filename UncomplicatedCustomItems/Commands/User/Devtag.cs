using CommandSystem;
using System;
using LabApi.Features.Wrappers;

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

            if (player is null)
            {
                response = "Can't use this command while not in the game!";
                return false;
            }

            if (!Plugin.Instance.Config.EnableCreditTags)
            {
                response = "Credit tags are disabled!";
                return false;
            }

            if (player.UserId != "76561199150506472@steam")
            {
                response = "Only UCI developers can run this command!";
                return false;
            }

            player.GroupName = "💻 UCI Lead Developer";
            player.GroupColor = "emerald";
            response = "Dev tag set!!";
            return true;
        }
    }
}
