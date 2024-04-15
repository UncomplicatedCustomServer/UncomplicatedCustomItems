using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API
{
    internal class Events
    {
        internal static Dictionary<ItemEvents, List<Func<CustomItemHandler, bool>>> EventList { get; set; } = new();

        public static void Register(ItemEvents Event, Func<CustomItemHandler, bool> Handler)
        {
            if (EventList.ContainsKey(Event))
            {
                EventList[Event].Add(Handler);
            } 
            else
            {
                EventList.Add(Event, new()
                {
                    Handler
                });
            }
        }

        public static void Unregister(ItemEvents Event, Func<CustomItemHandler, bool> Handler)
        {
            if (EventList.ContainsKey(Event))
            {
                EventList[Event].Remove(Handler);
            }
        }

        public static void Unregister(ItemEvents Event)
        {
            EventList[Event] = new();
        }

        public static void Unregister()
        {
            EventList.Clear();
        }

        internal static bool _Trigger(ItemEvents Event, CustomItemHandler Handler)
        {
            if (EventList.ContainsKey(Event))
            {
                bool Allowed = true;
                foreach (Func<CustomItemHandler, bool> Action in EventList[Event])
                {
                    Allowed &= Action(Handler);
                }
                return Allowed;
            }
            return true;
        }
    }
}