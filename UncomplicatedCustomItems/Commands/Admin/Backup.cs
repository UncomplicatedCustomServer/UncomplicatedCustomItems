using System;
using CommandSystem;
using UncomplicatedCustomItems.API.Features.Networking;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Backup : Subcommand
    {
        public override string Name { get; } = "backup";
        public override string Description { get; } = "Backups your CustomItems";
        public override string VisibleArgs { get; } = "[Upload|Download]";
        public override int RequiredArgsCount { get; } = 1;
        public override string RequiredPermission { get; } = "uci.backup";
        public override string[] Aliases { get; } = ["back"];

        public override bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (args.Count < 1)
            {
                response = "This command requires atleast one argument! (Upload or Download)";
                return false;
            }

            string customCode = args.Count > 1 ? args.At(1) : string.Empty;
            switch (args.At(0).ToLower())
            {
                case "upload":
                        BackupUploadRequest upload = new();
                        upload.SendRequest();
                        response = "Beginning backup upload...";
                        return true;

                case "download":
                    BackupDownloadRequest download = new();
                    download.SendRequest();
                    response = "Beginning backup download...";
                    return true;

                default:
                    response = "Available arguments are Download and Upload.";
                    return false;
            }
        }
    }
}