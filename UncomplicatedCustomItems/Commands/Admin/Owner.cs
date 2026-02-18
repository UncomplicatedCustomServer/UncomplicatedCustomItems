using CommandSystem;
using MEC;
using System.Collections.Generic;
using System.Net;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Owner : ISubcommand
    {
        public string Name { get; } = "owner";

        public string Description { get; } = "Get the 'Server Owner' role on our Discord server";

        public string RequiredPermission { get; } = "uci.owner";

        public string VisibleArgs { get; } = "Discord ID";

        public int RequiredArgsCount { get; } = 1;

        public string[] Aliases { get; } = [""];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            HttpStatusCode outstatus = new();

            Timing.RunCoroutine(Plugin.HttpManager.AddServerOwner(arguments[0], status =>
            {
                outstatus = status;
            }));

            response = outstatus switch
            {
                HttpStatusCode.OK => $"The request has been accepted!\nNow {arguments[0]} will be flagged as Server Owner!",
                HttpStatusCode.Forbidden => "Sorry but your server seems to not be on the public list!\nRetry in three minutes if you think that this is an error!",
                HttpStatusCode.BadRequest => "It seems that the Discord user ID is invalid!",
                HttpStatusCode.InternalServerError => "The central server is having some issues, please report this message to the Discord as a bug!",
                _ => $"The response seems to be invalid.\nRaw format: {outstatus}",
            };
            return true;
        }
    }
}
