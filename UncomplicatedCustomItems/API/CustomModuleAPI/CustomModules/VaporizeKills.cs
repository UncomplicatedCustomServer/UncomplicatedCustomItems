using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class VaporizeKills : CustomModuleBase
    {
        public override string Name => "VaporizeKills";

        public void Execute(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerDyingEventArgs ev && Utilities.TryGetSummonedCustomItem(ev.Attacker?.CurrentItem.Serial ?? 0, out var item))
            {
                LogManager.Silent("Name | Id | CustomFlag(s)");
                LogManager.Silent($"{item.CustomItem.Name} - {item.CustomItem.Id}");
                LogManager.Debug($"Vaporizing {ev.Player.Nickname}");
                ev.Player.Vaporize(ev.Attacker);
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.Dying += Execute;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.Dying -= Execute;
        }
    }
}