using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.Interfaces;
using EventSource = Exiled.Events.Handlers.Server;

namespace UncomplicatedCustomItems.Events.Internal
{
    internal class Server
    {
        public static void Register()
        {
            EventSource.RoundStarted += SpawnItemsOnRoundStarted;
        }

        public static void Unregister()
        {
            EventSource.RoundStarted -= SpawnItemsOnRoundStarted;
        }

        /// <summary>
        /// Spawn items on round started
        /// </summary>
        private static void SpawnItemsOnRoundStarted() 
        {
            foreach (ICustomItem CustomItem in Manager.Items.Values)
            {
                if (CustomItem.Spawn is not null && CustomItem.Spawn.DoSpawn)
                {
                    for (uint count = 0; count < CustomItem.Spawn.Count; count++)
                    {
                        Utilities.SummonCustomItem(CustomItem);
                    }
                }
            }
        }
    }
}
