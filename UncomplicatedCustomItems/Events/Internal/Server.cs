using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Interfaces;
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
            foreach (ICustomItem CustomItem in CustomItem.List)
            {
                LogManager.Debug($"{CustomItem.Name} DoSpawn is set to {CustomItem.Spawn.DoSpawn}");
                if (CustomItem.Spawn is not null && CustomItem.Spawn.DoSpawn)
                {
                    for (uint count = 0; count < CustomItem.Spawn.Count; count++)
                    {
                        LogManager.Debug($"Spawning {CustomItem.Name} ({count + 1}/{CustomItem.Spawn.Count})");
                        Utilities.SummonCustomItem(CustomItem);
                    }
                }
            }
            Timing.CallDelayed(1f, () =>
            {
                foreach (SummonedCustomItem customItem in SummonedCustomItem.List)
                {
                    foreach (Pickup pickup in Pickup.List)
                    {
                        if (pickup.Serial == customItem.Serial)
                        {
                            pickup.GameObject.transform.localScale = customItem.CustomItem.Scale;
                            pickup.Weight = customItem.CustomItem.Weight;
                        }
                    }
                }
            });
        }
    }
}
