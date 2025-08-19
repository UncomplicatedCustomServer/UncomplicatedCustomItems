namespace UncomplicatedCustomItems.Commands
{
    public class SilentCommandSender : CommandSender
    {
        public override string SenderId => "UncomplicatedCustomItems";
        public override string Nickname => "UncomplicatedCustomItems";
        public override ulong Permissions => ulong.MaxValue;
        public override byte KickPower => byte.MaxValue;
        public override bool FullPermissions => true;

        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay) { }

        public override void Print(string text) { }

        public override bool Available() => true;

        public override void Respond(string message, bool success = true) { }
    }
}