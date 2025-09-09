using System.Collections.Generic;
using System.Threading.Tasks;
using CommandSystem;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands
{
    public class PresenceDebug : ISubcommand
    {
        public string Name { get; } = "sendpresence";

        public string Description { get; } = "";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string[] RequiredPermission { get; } = ["uci.sendpresence"];

        public string[] Aliases { get; } = ["sp"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            Task.Run(() => Plugin.HttpManager.SendPresenceOnceAsync());
            response = "Sent";
            return true;
        }
    }
}