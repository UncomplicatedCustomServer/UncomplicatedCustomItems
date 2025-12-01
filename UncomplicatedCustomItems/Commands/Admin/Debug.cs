using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Components;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.Commands.Admin
{
    public class Debug : ISubcommand
    {
        public string Name { get; } = "debug";
        public string Description { get; } = "use UCI's debug commands";
        public string VisibleArgs { get; } = "";
        public int RequiredArgsCount { get; } = 1;
        public string RequiredPermission { get; } = "uci.debug";
        public string[] Aliases { get; } = ["deb"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "You must be a Player to use this command!";
                return false;
            }

            DebugUI uI;
            switch (args[0].ToLower())
            {
                case "ui":
                    if (!player.GameObject.TryGetComponent<DebugUI>(out uI))
                    {
                        player.GameObject.AddComponent<DebugUI>().Init(player);
                        uI = player.GameObject.GetComponent<DebugUI>();
                    }

                    switch (args[1].ToLower().Replace(" ", "_"))
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