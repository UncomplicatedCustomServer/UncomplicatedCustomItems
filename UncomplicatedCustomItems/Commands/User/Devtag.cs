using CommandSystem;
using System;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features.Networking;
using static UncomplicatedCustomItems.API.Features.Networking.CreditsRequest;

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
            if (!Player.TryGet(sender, out var player))
            {
                response = "Can't use this command while not in the game!";
                return false;
            }

            if (!Plugin.Instance.Config.EnableCreditTags)
            {
                response = "Credit tags are disabled!";
                return false;
            }

            CreditTag credits = CreditsRequest.GetCreditTag(player);
            if (credits is null)
            {
                response = "You do not have a credit tag!";
                return true;
            }

            CreditsRequest.ApplyCreditTag(player);
            response = $"Applied Credit Tag with name: {credits.Role} color: {credits.Color}";
            return true;
        }
    }
}
