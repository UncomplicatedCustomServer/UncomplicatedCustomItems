using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class CantDrop : CustomModuleBase
    {
        public override string Name => "CantDrop";

        public string HintOrBroadcast { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public uint Duration { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;

            if (CustomItem == null)
                return;

            LogManager.Debug("Running CantDrop");
            if (eventArgs is PlayerDroppingItemEventArgs ev)
            {
                ev.IsAllowed = false;
                string formattedMsg = Message.Replace("%name%", CustomItem.Name);

                if (string.Equals(HintOrBroadcast, "Broadcast", StringComparison.OrdinalIgnoreCase))
                {
                    LogManager.Silent("Name | Id");
                    LogManager.Silent($"{CustomItem.Name} - {CustomItem.Id}");
                    LogManager.Debug($"Sending CantDrop Broadcast to {ev.Player.Nickname}\nBroadcast: {formattedMsg}");
                    ev.Player.SendBroadcast(formattedMsg, (ushort)Duration, Broadcast.BroadcastFlags.Normal, true);
                }
                else if (string.Equals(HintOrBroadcast, "Hint", StringComparison.OrdinalIgnoreCase))
                {
                    LogManager.Silent("Name | Id");
                    LogManager.Silent($"{CustomItem.Name} - {CustomItem.Id}");
                    LogManager.Debug($"Sending CantDrop Hint to {ev.Player.Nickname}\nHint: {formattedMsg}");
                    ev.Player.SendHint(formattedMsg, Duration);
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