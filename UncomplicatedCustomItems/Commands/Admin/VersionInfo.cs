using System.Text;
using System;
using CommandSystem;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class VersionInfo : Subcommand
    {
        public override string Name { get; } = "versioninfo";

        public override string Description { get; } = "Gets the information about the installed UCI version";

        public override string RequiredPermission { get; } = "uci.versioninfo";

        public override string VisibleArgs { get; } = "";

        public override int RequiredArgsCount { get; } = 0;

        public override string[] Aliases { get; } = ["vi"];

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            VersionInfoV2? info = VersionManager.VersionInfo;
            if (info == null)
            {
                response = "Failed to get version info.";
                return false;
            }

            sb.AppendLine();
            sb.AppendLine($"Version name: {info.CodeName}");
            sb.AppendLine($"Version number: {info.Version}");

            if (info.PreRelease)
                sb.AppendLine($"Prerelease: {info.PreRelease}");

            if (info.Recalled)
            {
                sb.AppendLine($"Recalled: {info.Recalled}");
                sb.AppendLine($"Recall reason: {info.RecallReason}");
            }

            if (info.ForceDebug)
                sb.AppendLine($"Debug forced: {info.ForceDebug}");

            response = sb.ToString();
            return true;
        }
    }
}