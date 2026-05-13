using CommandSystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Net;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Features.Networking;
using UncomplicatedCustomItems.API.Interfaces;
using static EncryptedChannelManager;

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
            string res = string.Empty;

            if (!Player.TryGet(sender, out var player))
            {
                response = "Cant use this command while not in the game!";
                return false;
            }

            AddServerOwnerRequest ownerrequest = new(player, arguments[0]);
            ownerrequest.SendRequest((request) => {
                HttpStatusCode status = (HttpStatusCode)request.responseCode;
                res = status switch
                {
                    HttpStatusCode.OK => $"The request has been accepted!\nNow {arguments[0]} will be flagged as an Server Owner!",
                    HttpStatusCode.Forbidden => "Sorry but your server seems to not be on the public list!\nRetry in three minutes if you think that this is an error!",
                    HttpStatusCode.BadRequest => "It seems that the Discord user ID is invalid!",
                    HttpStatusCode.InternalServerError => "The central server is having some issues, please report this message to the Discord as a bug!",
                    _ => $"The response seems to be invalid.\nRaw format: {status}",
                };

                LogManager.Info(res);
                EncryptedMessage msg = new(EncryptedChannel.RemoteAdmin, res, 1);
                player.ReferenceHub.encryptedChannelManager.TrySendMessageToClient(res, EncryptedChannel.RemoteAdmin);
            });

            response = "You should be seeing the response soon...";
            return true;
        }
    }
}
