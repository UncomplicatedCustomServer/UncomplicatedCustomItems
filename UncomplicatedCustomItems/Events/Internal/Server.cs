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
        private static void SpawnItemsOnRoundStarted() { }
    }
}
