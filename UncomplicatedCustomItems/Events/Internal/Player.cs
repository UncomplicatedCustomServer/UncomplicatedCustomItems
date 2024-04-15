using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomItems.API.Extensions;
using EventSource = Exiled.Events.Handlers.Player;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal static class Player
    {
        public static void Register()
        {
            EventSource.UsingItem += CancelUsingCustomItemOnUsingItem;
        }

        public static void Unregister()
        {
            EventSource.UsingItem -= CancelUsingCustomItemOnUsingItem;
        }

        /// <summary>
        /// Cancel using if it is custom item
        /// </summary>
        /// <param name="ev"></param>
        public static void CancelUsingCustomItemOnUsingItem(UsingItemEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                return;
            }

            ev.IsAllowed = !ev.Item.IsCustomItem();
        }
    }
}
