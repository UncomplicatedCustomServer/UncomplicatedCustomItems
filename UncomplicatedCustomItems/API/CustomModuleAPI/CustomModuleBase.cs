using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.Interfaces;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.CustomModuleAPI
{
    public abstract class CustomModuleBase
    {
        public ICustomItem CustomItem { get; set; }
        public abstract string Name { get; }
        public virtual List<string> RequiredArguments => new();
        public virtual List<Dictionary<object, object>> Arguments { get; set; }
        public virtual void Run(EventArgs eventArgs)
        {
            LogManager.Debug($"Running {Name}");
            if (eventArgs is IPlayerEvent playerEvent && playerEvent.Player.CurrentItem is not null)
            {
                if (!Utilities.TryGetSummonedCustomItem(playerEvent.Player.CurrentItem.Serial, out var playeritem))
                    return;

                if (playeritem.CustomItem != CustomItem)
                    return;
            }

            if (eventArgs is IItemEvent itemEvent)
            {
                if (!Utilities.TryGetSummonedCustomItem(itemEvent.Item.Serial, out var item))
                    return;

                if (item.CustomItem != CustomItem)
                    return;
            }
        }


        public virtual void Run() { }

        public virtual void RegisterEvents() { }
        public virtual void UnregisterEvents() { }

        public virtual void OnAdded(SummonedCustomItem item) { }
        public virtual void OnRegistered() { }
        public virtual void OnDestroyed() { }
        public bool HasFlagFast(TriggerOn flags, TriggerOn flag) => (flags & flag) == flag;
    }
}