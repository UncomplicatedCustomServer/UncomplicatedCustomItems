using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Debug : Subcommand
    {
        public override string Name { get; } = "debug";
        public override string Description { get; } = "use UCI's debug commands";
        public override string VisibleArgs { get; } = "";
        public override int RequiredArgsCount { get; } = 1;
        public override string RequiredPermission { get; } = "uci.debug";
        public override string[] Aliases { get; } = ["deb"];

        public override bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player? player))
            {
                response = "You must be a Player to use this command!";
                return false;
            }

            DebugUI uI;
            switch (args.At(0).ToLower())
            {
                case "ui":
                    if (!player.GameObject!.TryGetComponent<DebugUI>(out uI))
                    {
                        player.GameObject?.AddComponent<DebugUI>().Init(player);
                        uI = player.GameObject?.GetComponent<DebugUI>()!;
                    }

                    switch (args.At(1).ToLower().Replace(" ", "_"))
                    {
                        case "all":
                            if (uI.ActiveSegments.Contains(Segment.All))
                            {
                                uI.RemoveSegment(Segment.All);
                                response = "Removed all segments";
                                return true;
                            }
                            else
                            {
                                uI.AddSegment(Segment.All);
                                response = "Added all segments";
                                return true;
                            }

                        case "total" or "totalitems" or "total_items":
                            if (uI.ActiveSegments.Contains(Segment.TotalItems))
                            {
                                uI.RemoveSegment(Segment.TotalItems);
                                response = "Removed 'Total Items' segment";
                                return true;
                            }
                            else
                            {
                                uI.AddSegment(Segment.TotalItems);
                                response = "Added 'Total Items' segment";
                                return true;
                            }

                        case "zone" or "zoneinfo" or "zoneinformation":
                            if (uI.ActiveSegments.Contains(Segment.ZoneInformation))
                            {
                                uI.RemoveSegment(Segment.ZoneInformation);
                                response = "Removed 'Zone Information' segment";
                                return true;
                            }
                            else
                            {
                                uI.AddSegment(Segment.ZoneInformation);
                                response = "Added 'Zone Information' segment";
                                return true;
                            }

                        case "base":
                            if (uI.ActiveSegments.Contains(Segment.Base))
                            {
                                uI.RemoveSegment(Segment.Base);
                                response = "Removed 'Base' segment";
                                return true;
                            }
                            else
                            {
                                uI.AddSegment(Segment.Base);
                                response = "Added 'Base' segment";
                                return true;
                            }

                        case "help":
                            response = "Valid arguments are: 'All', 'TotalItems', 'ZoneInformation', 'Base'";
                            return true;

                        default:
                            if (uI.ActiveSegments.Contains(Segment.Base))
                            {
                                uI.RemoveSegment(Segment.Base);
                                response = "Removed 'Base' segment";
                                return true;
                            }
                            else
                            {
                                uI.AddSegment(Segment.Base);
                                response = "Added 'Base' segment";
                                return true;
                            }
                    }

                default:
                    response = "Arguments are invalid! valid arguments are: 'UI'";
                    return false;

            }
        }
    }
}