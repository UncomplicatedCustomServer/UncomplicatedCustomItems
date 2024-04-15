using Exiled.API.Features;
using Exiled.API.Features.Items;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    internal class CustomItemHandler
    {
        public ICustomItem CustomItem { get; }

        public Player Player { get; }

        public Item Item { get; }

        public ushort Serial { get; }

        public Dictionary<ItemEvents, Func<CustomItemHandler, bool>> InternalEvents { get; }

        public CustomItemHandler(ICustomItem customItem, Player player)
        {
            CustomItem = customItem;
            Player = player;
            Item = Item.Create(customItem.Item, player);
            Serial = Item.Serial;
            InternalEvents = new();
        }

        public void AddEventListener(ItemEvents Event, Func<CustomItemHandler, bool> Action)
        {
            InternalEvents[Event] = Action;
        }

        public void RemoveEventListener(ItemEvents Event)
        {
            InternalEvents.Remove(Event);
        }

        internal bool TriggerEvent(ItemEvents Event)
        {
            bool Allowed = true;
            if (CustomItem.EventsEnabled)
            {
                if (InternalEvents.ContainsKey(Event))
                {
                    Allowed &= InternalEvents[Event](this);
                }
                Allowed &= Events._Trigger(Event, this);
            }

            if (CustomItem.Event == Event)
            {
                if (CustomItem.Command is not null && CustomItem.Command != string.Empty)
                {
                    Server.ExecuteCommand(CustomItem.Command.Replace("%id%", Player.Id.ToString()), Player.Sender);
                }

                Helper.Helper.ParseResponse(Player, CustomItem.Response);
            }

            return Allowed;
        }
    }
}
