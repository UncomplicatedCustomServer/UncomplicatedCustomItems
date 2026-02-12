using System.Collections.Generic;
using CommandSystem;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Backup : ISubcommand
    {
        public string Name { get; } = "backup";
        public string Description { get; } = "Backups your CustomItems";
        public string VisibleArgs { get; } = "[Upload|Download]";
        public int RequiredArgsCount { get; } = 1;
        public string RequiredPermission { get; } = "uci.backup";
        public string[] Aliases { get; } = ["back"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            switch (args[0].ToLower())
            {
                case "upload":
                    if (CustomItemBackupSystem.Online)
                    {
                        CustomItemBackupSystem.Upload();
                        response = "Beginning backup upload...";
                        return true;
                    }
                    else
                    {
                        response = "Failed to find Backup endpoint";
                        return false;
                    }

                case "download":
                    if (CustomItemBackupSystem.Online)
                    {
                        CustomItemBackupSystem.Download();
                        response = "Beginning backup download...";
                        return true;
                    }
                    else
                    {
                        response = "Failed to find Backup endpoint";
                        return false;
                    }

                default:
                    response = "Available arguments are Download and Upload.";
                    return false;
            }
        }
    }
}