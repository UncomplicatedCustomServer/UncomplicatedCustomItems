using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.CustomModuleAPI
{
    public abstract class CustomModuleBase
    {
        [YamlIgnore]
        public ICustomItem? CustomItem { get; set; }

        [YamlIgnore]
        public abstract string Name { get; }

        [YamlIgnore]
        public virtual List<string> RequiredArguments => [];

        [YamlIgnore]
        public virtual List<Dictionary<object, object>> Arguments { get; set; } = [];

        public virtual void Run(EventArgs eventArgs) { }

        public bool Check(EventArgs eventArgs)
        {
            if (eventArgs is IItemEvent itemEvent && itemEvent.Item != null)
                return MatchesSerial(itemEvent.Item.Serial);

            if (eventArgs is IPickupEvent pickupEvent && pickupEvent.Pickup != null)
                return MatchesSerial(pickupEvent.Pickup.Serial);

            if (eventArgs is IPlayerEvent playerEvent)
            {
                if (Matches(playerEvent.Player))
                    return true;

                return eventArgs switch
                {
                    PlayerHurtEventArgs hurt => Matches(hurt.Attacker),
                    PlayerDeathEventArgs death => Matches(death.Attacker),
                    PlayerDyingEventArgs dying => Matches(dying.Attacker),
                    _ => false
                };
            }

            return true;
        }

        public bool Check(Item item) => MatchesSerial(item.Serial);


        private bool Matches(Player? player) => player?.CurrentItem != null && MatchesSerial(player.CurrentItem.Serial);

        private bool MatchesSerial(ushort serial) => Utilities.TryGetSummonedCustomItem(serial, out var item) && item?.CustomItem == CustomItem;

        public virtual void Run() { }

        public virtual void RegisterEvents() { }
        public virtual void UnregisterEvents() { }

        public virtual void OnAdded(SummonedCustomItem item) { }
        public virtual void OnRegistered() { }
        public virtual void OnDestroyed() { }
        public bool HasFlagFast(TriggerOn flags, TriggerOn flag) => (flags & flag) == flag;
    }
}