using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class CantDrop : CustomModuleBase
    {
        public override string Name => "CantDrop";
        public override List<string> RequiredArguments => 
        [
            "HintOrBroadcast",
            "Message",
            "Duration"
        ];

        public string HintOrBroadcast { get; set; }
        public string Message { get; set; }
        public uint Duration { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("HintOrBroadcast", out var hob))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} HintOrBroadcast is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<string>("Message", out var msg))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Message is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<uint>("Duration", out var dur))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Duration is not a valid uint!");
                    return;
                }

                HintOrBroadcast = hob;
                Message = msg;
                Duration = dur;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;

            LogManager.Debug("Running CantDrop");
            if (eventArgs is PlayerDroppingItemEventArgs ev)
            {
                ev.IsAllowed = false;
                if (string.Equals(HintOrBroadcast, "Broadcast", StringComparison.OrdinalIgnoreCase))
                {
                    LogManager.Silent("Name | Id");
                    LogManager.Silent($"{CustomItem.Name} - {CustomItem.Id}");
                    LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.Nickname}\nBroadcast: {Message.Replace("%name%", CustomItem.Name)}");
                    ev.Player.SendBroadcast($"{Message.Replace("%name%", CustomItem.Name)}", (ushort)Duration, Broadcast.BroadcastFlags.Normal, true);
                }
                else if (string.Equals(HintOrBroadcast, "Hint", StringComparison.OrdinalIgnoreCase))
                {
                    LogManager.Silent("Name | Id");
                    LogManager.Silent($"{CustomItem.Name} - {CustomItem.Id}");
                    LogManager.Debug($"Sending CantDrop Hint to {ev.Player.Nickname}\nHint: {Message.Replace("%name%", CustomItem.Name)}");
                    ev.Player.SendHint($"{Message.Replace("%name%", CustomItem.Name)}", Duration);
                }
            }
        }

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            PlayerEvents.DroppingItem += Run;
            LogManager.Debug("Registered CantDrop Events");
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.DroppingItem -= Run;
        }
    }
}