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
<<<<<<< HEAD
        private static void SpawnItemsOnRoundStarted() { }
=======
        private static void SpawnItemsOnRoundStarted()
        {
            SpawnCustomItemsList(Plugin.Instance.Config.CustomItems.Values);
            SpawnCustomItemsList(Plugin.Instance.Config.CustomArmors.Values);
            SpawnCustomItemsList(Plugin.Instance.Config.CustomWeapons.Values);
            SpawnCustomItemsList(Plugin.Instance.Config.CustomKeycards.Values);
        }

        /// <summary>
        /// Spawn items in enumerable
        /// </summary>
        /// <param name="customThings"></param>
        private static void SpawnCustomItemsList(IEnumerable<SerializableThing> customThings)
        {
            foreach (var customItem in customThings)
            {
                foreach (var spawnPoint in customItem.SpawnPoint)
                {
                    if (customItem.SpawnPoint is null || spawnPoint.Chance == 0)
                    {
                        continue;
                    }

                    var chance = UnityEngine.Random.Range(0, 100);

                    if (spawnPoint.Chance != 100 && spawnPoint.Chance > chance)
                    {
                        continue;
                    }

                    customItem.Create(null).Spawn(spawnPoint.Location.GetPosition() + spawnPoint.Position);
                }            
            }
        }
>>>>>>> 2ff79ee1861a7035b48d099ce36d6c27c501525a
    }
}
