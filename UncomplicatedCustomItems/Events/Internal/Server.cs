using System.Linq;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.Events.Arguments.CustomItemEvents;
using EventSource = LabApi.Events.Handlers.ServerEvents;

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
        public static void SpawnItemsOnRoundStarted()
        {
            foreach (ICustomItem customItem in CustomItem.List)
            {
                if (customItem.Item is ItemType.SCP330 && customItem.CustomData is ICandyData data && !data.AllowSpawningAsItem)
                    continue;

                LogManager.Debug($"{customItem.Name} DoSpawn is set to {customItem.Spawn.DoSpawn}");
                if (customItem.Spawn is not null && customItem.Spawn.DoSpawn)
                {
                    for (uint count = 0; count < customItem.Spawn.Count; count++)
                    {
                        LogManager.Debug($"Spawning {customItem.Name} ({count + 1}/{customItem.Spawn.Count})");
                        Utilities.SummonCustomItem(customItem);
                    }
                }
            }

            if (APICustomItem.List.Count() > 0)
            {
                foreach (APICustomItem item in APICustomItem.List)
                {
                    if (item is CustomCandy data && !data.AllowSpawningAsItem)
                        continue;

                    LogManager.Debug($"{item.Name} DoSpawn is set to {item.Spawn}");
                    if (item.Spawn)
                    {
                        for (uint count = 0; count < item.AmountToSpawn; count++)
                        {
                            float chance = UnityEngine.Random.Range(0f, 101f);
                            if (chance >= item.ChanceToSpawn)
                            {
                                LogManager.Debug($"Spawning {item.Name} ({count + 1}/{item.AmountToSpawn})");
                                APICustomItem.SummonItem(item);
                            }
                        }
                    }
                }
            }
        }
    }
}
