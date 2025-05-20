using LabApi.Features.Wrappers;
using UnityEngine;

namespace UncomplicatedCustomItems.Extensions
{
    internal static class PickupExtensions
    {
        public static Pickup CreateAndSpawn(this ItemType item, Vector3 pos)
        {
            Pickup pickup = Pickup.Create(item, pos);
            pickup.Spawn();
            return pickup;
        }
    }
}
