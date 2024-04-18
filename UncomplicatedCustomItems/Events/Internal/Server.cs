using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Serializable;
using UnityEngine;
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
            SpawnCustomItemsList(Plugin.Instance.Config.CustomItems.Values);
            SpawnCustomItemsList(Plugin.Instance.Config.CustomArmors.Values);
            SpawnCustomItemsList(Plugin.Instance.Config.CustomWeapons.Values);
        }

        /// <summary>
        /// Spawn items in enumerable
        /// </summary>
        /// <param name="customThings"></param>
        private static void SpawnCustomItemsList(IEnumerable<SerializableThing> customThings)
        {
            foreach (var customItem in customThings)
            {
                if (customItem.SpawnPoint is null || customItem.SpawnPoint.Chance == 0)
                {
                    continue;
                }

                var spawnPoint = customItem.SpawnPoint;

                var chance = UnityEngine.Random.Range(0, 100);

                if (spawnPoint.Chance != 100 && spawnPoint.Chance > chance)
                {
                    continue;
                }

                customItem.Create(null).Spawn(spawnPoint.Location.GetPosition() + spawnPoint.Position);
            }
        }
    }
}
