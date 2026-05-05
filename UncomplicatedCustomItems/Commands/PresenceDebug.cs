using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands
{
    public class PresenceDebug : ISubcommand
    {
        public string Name { get; } = "sendpresence";

        public string Description { get; } = "";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "uci.sendpresence";

        public string[] Aliases { get; } = ["sp"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Plugin.Instance.Config.Debug)
            {
                response = "Debug is disabled.";
                return false;
            }

            if (Player.Host.GameObject.TryGetComponent<Presence>(out var presence))
            {
                presence.SendPresence();
                if (presence.LastUploadSucceeded)
                {
                    response = "Sent successfully.";
                    return true;
                }
                else
                {
                    response = "Failed to send!";
                    return true;
                }
            }
            else
            {
                response = "Failed to find the Presence component on host";
                return false;
            }
        }
    }
}