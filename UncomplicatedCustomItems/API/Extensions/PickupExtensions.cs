using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class PickupExtensions
    {
        /// <summary>
        /// Create a spawn a <see cref="Pickup"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Pickup? CreateAndSpawn(this ItemType item, Vector3 pos, Quaternion rot = default, Vector3 scale = default)
        {
            Pickup? pickup = Pickup.Create(item, pos, rot, scale == default ? Vector3.one : scale);
            pickup?.Spawn();
            return pickup;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Pickup"/> is a CustomItem.
        /// </summary>
        /// <param name="pickup">The pickup to check.</param>
        /// <returns><c>true</c> if the pickup is a <see cref="ICustomItem"/>; otherwise, <c>false</c>.</returns>
        public static bool IsCustomItem(this Pickup pickup) => Utilities.IsCustomItem(pickup.Serial);

        /// <summary>
        /// Determines whether the specified <see cref="Pickup"/> is a <see cref="SummonedCustomItem"/>.
        /// </summary>
        /// <param name="pickup">The pickup to check.</param>
        /// <returns><c>true</c> if the pickup is a <see cref="SummonedCustomItem"/>; otherwise, <c>false</c>.</returns>
        public static bool IsSummonedCustomItem(this Pickup pickup) => Utilities.IsSummonedCustomItem(pickup.Serial);

        /// <summary>
        /// Determines whether the specified <see cref="Item"/> is a summoned custom item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is a summoned custom item; otherwise, <c>false</c>.</returns>
        public static bool IsSummonedAPICustomItem(this Pickup pickup) => SummonedAPICustomItem.TryGet(pickup.Serial, out _);

#nullable enable

        /// <summary>
        /// Attempts to retrieve the <see cref="SummonedCustomItem"/> associated with the specified <see cref="Pickup"/>.
        /// </summary>
        /// <param name="pickup">The pickup to query.</param>
        /// <returns>
        /// The corresponding <see cref="SummonedCustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static SummonedCustomItem? TryGetSummonedCustomItem(this Pickup pickup) => Utilities.TryGetSummonedCustomItem(pickup.Serial, out SummonedCustomItem? customItem) ? customItem : null;
        /// <summary>
        /// Attempts to retrieve the <see cref="ICustomItem"/> associated with the specified <see cref="Pickup"/>.
        /// </summary>
        /// <param name="pickup">The pickup to query.</param>
        /// <returns>
        /// The corresponding <see cref="ICustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static ICustomItem? TryGetCustomItem(this Pickup pickup) => Utilities.TryGetCustomItem(pickup.Serial, out ICustomItem customItem) ? customItem : null;

#nullable disable

        /// <summary>
        /// Compares two <see cref="Pickup"/> instances to determine if they refer to the same CustomItem definition.
        /// </summary>
        /// <param name="pickup1">The first pickup to compare.</param>
        /// <param name="pickup2">The second pickup to compare.</param>
        /// <returns>
        /// <c>true</c> if both pickups refer to the same <see cref="ICustomItem"/> definition; otherwise, <c>false</c>.
        /// </returns>
        public static bool CompareCustomItems(this Pickup pickup1, Pickup pickup2) => TryGetCustomItem(pickup1) == TryGetCustomItem(pickup2);

        /// <summary>
        /// Compares two <see cref="Pickup"/> instances to determine if they are the same <see cref="SummonedCustomItem "/>.
        /// </summary>
        /// <param name="pickup1">The first pickup to compare.</param>
        /// <param name="pickup2">The second pickup to compare.</param>
        /// <returns>
        /// <c>true</c> if both pickups refer to the same <see cref="SummonedCustomItem "/> instance; otherwise, <c>false</c>.
        /// </returns>
        public static bool CompareSummonedCustomItems(this Pickup pickup1, Pickup pickup2) => TryGetSummonedCustomItem(pickup1) == TryGetSummonedCustomItem(pickup2);
    }
}
