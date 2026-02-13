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
            if (args.Count < 1)
            {
                response = "This command requires atleast one argument! (Upload or Download)";
                return false;
            }

            string customCode = args.Count > 1 ? args[1] : string.Empty;
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
                        CustomItemBackupSystem.Download(customCode);
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