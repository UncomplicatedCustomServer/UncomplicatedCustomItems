using CommandSystem;
using System;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Struct;

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

            if (!Plugin.HttpManager.Credits.ContainsKey(player.UserId))
            {
                response = "You do not have a credit tag!";
                return true;
            }

            Plugin.HttpManager.ApplyCreditTag(player);
            Triplet<string, string, bool> credits = Plugin.HttpManager.Credits[player.UserId];
            response = $"Applied Credit Tag with name: {credits.First} color: {credits.Second}";
            return true;
        }
    }
}
