using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
