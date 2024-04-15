using CommandSystem;
using Exiled.API.Features;
using System;

namespace UncomplicatedCustomItems.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class UCIUse : ParentCommand
    {
        public UCIUse() => LoadGeneratedCommands();

        public override string Command { get; } = "uciuse";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Use a custom item";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "";
            Player Player = Player.Get(sender);
            if (Helper.Helper.IsCustomItem(Player, Player.CurrentItem))
            {
                Helper.Helper.GetCustomItem(Player, Player.CurrentItem).TriggerEvent(ItemEvents.Command);
            }
            return true;
        }
    }
}
