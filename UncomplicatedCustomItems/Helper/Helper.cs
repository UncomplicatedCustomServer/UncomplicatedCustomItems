using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.Helper
{
    internal class Helper
    {
        public static bool RegisterItem(ICustomItem Item)
        {
            if (Plugin.Items.ContainsKey(Item.Id))
            {
                Log.Warn($"A custom item with Id {Item.Id} and name {Item.Name} tried to be registered but another item with the id {Item.Id} already exists!");
                return false;
            }

            Plugin.Items.Add(Item.Id, Item);
            return true;
        }

        public static bool UnregisterItem(ICustomItem Item)
        {
            if (!Plugin.Items.ContainsKey(Item.Id))
            {
                Log.Warn($"A custom item with Id {Item.Id} and name {Item.Name} does not exists, so can't be unregistered!");
                return false;
            }

            Plugin.Items.Remove(Item.Id);
            return true;
        }

        public static List<CustomItemHandler> GetCustomItemsFromPlayer(Player Player)
        {
            if (!Plugin.ItemDictionary.ContainsKey(Player.Id))
            {
                return new();
            }

            return Plugin.ItemDictionary[Player.Id];
        }

        public static bool HasCustomItem(Player Player, CustomItemHandler Handler)
        {
            return GetCustomItemsFromPlayer(Player).Contains(Handler);
        }

        public static bool HasCustomItem(Player Player, ICustomItem Base)
        {
            return GetCustomItem(Player, Base) != default;
        }

        public static bool HasCustomItem(Player Player, uint Base)
        {
            return GetCustomItem(Player, Base) != default;
        }

        public static bool IsCustomItem(Player Player, Item Item)
        {
            return Plugin.ItemDictionary[Player.Id].Where(item => item.Item.Serial == Item.Serial).Count() > 0;
        }

        public static CustomItemHandler GetCustomItem(Player Player, Item Item)
        {
            return Plugin.ItemDictionary[Player.Id].Where(item => item.Item.Serial == Item.Serial).FirstOrDefault();
        }

        public static CustomItemHandler GetCustomItem(Player Player, ICustomItem Base)
        {
            return Plugin.ItemDictionary[Player.Id].Where(customitem => customitem.CustomItem == Base).FirstOrDefault();
        }

        public static CustomItemHandler GetCustomItem(Player Player, uint Base)
        {
            return Plugin.ItemDictionary[Player.Id].Where(customitem => customitem.CustomItem.Id == Base).FirstOrDefault();
        }

        public static CustomItemHandler Give(Player Player, ICustomItem Item)
        {
            CustomItemHandler Handler = new(Item, Player);

            // Add item to dictionary
            if (!Plugin.ItemDictionary.ContainsKey(Player.Id))
            {
                Plugin.ItemDictionary.Add(Player.Id, new()
                {
                    Handler
                });
            } 
            else
            {
                Plugin.ItemDictionary[Player.Id].Add(Handler);
            }

            return Handler;
        }

        public static void Remove(Player Player, CustomItemHandler Handler)
        {
            if (HasCustomItem(Player, Handler))
            {
                // Can remove, let's do that
                Player.RemoveItem(Handler.Item);
                Plugin.ItemDictionary[Player.Id].Remove(Handler);
            }
        }

        public static void Remove(Player Player, ICustomItem Handler)
        {
            if (HasCustomItem(Player, Handler))
            {
                // Can remove, let's do that
                Player.RemoveItem(GetCustomItem(Player, Handler).Item);
                Plugin.ItemDictionary[Player.Id].Remove(GetCustomItem(Player, Handler));
            }
        }

        public static void Remove(Player Player, uint Handler)
        {
            if (HasCustomItem(Player, Handler))
            {
                // Can remove, let's do that
                Player.RemoveItem(GetCustomItem(Player, Handler).Item);
                Plugin.ItemDictionary[Player.Id].Remove(GetCustomItem(Player, Handler));
            }
        }

        internal static bool HandleItemEvent(ItemEvents Event, IItemEvent ItemEvent, Player Player)
        {
            CustomItemHandler Handler = Plugin.ItemDictionary[Player.Id].Where(item => item.Item.Serial == ItemEvent.Item.Serial).FirstOrDefault();
            if (Handler != default)
            {
                return Handler.TriggerEvent(Event);
            }
            return true;
        }

        internal static void ParseResponse(Player Player, IResponse Response)
        {
            if (Response.ConsoleMessage is not null && Response.ConsoleMessage.Length > 1) // FUCK 1 char messages!
            {
                Player.SendConsoleMessage(Response.ConsoleMessage, string.Empty);
            }

            if (Response.BroadcastMessage.Length > 1 && Response.BroadcastDuration > 0)
            {
                Player.Broadcast(Response.BroadcastDuration, Response.BroadcastMessage);
            }

            if (Response.HintMessage.Length > 1 && Response.HintDuration > 0)
            {
                Player.ShowHint(Response.HintMessage, Response.HintDuration);
            }
        }
    }
}
