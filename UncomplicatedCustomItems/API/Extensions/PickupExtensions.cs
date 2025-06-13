using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API;
using UncomplicatedCustomItems.API.Interfaces;
using Exiled.API.Features.Pickups;

namespace UncomplicatedCustomItems.API.Extensions
{
    internal static class PickupExtensions
    {
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
        /// Attempts to retrieve the <see cref="SummonedCustomItem"/> associated with the specified <see cref="Pickup"/>.
        /// </summary>
        /// <param name="pickup">The pickup to query.</param>
        /// <returns>
        /// The corresponding <see cref="SummonedCustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static SummonedCustomItem TryGetSummonedCustomItem(this Pickup pickup)
        {
            return Utilities.TryGetSummonedCustomItem(pickup.Serial, out SummonedCustomItem customItem) ? customItem : null;
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="ICustomItem"/> associated with the specified <see cref="Pickup"/>.
        /// </summary>
        /// <param name="pickup">The pickup to query.</param>
        /// <returns>
        /// The corresponding <see cref="ICustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static ICustomItem TryGetCustomItem(this Pickup pickup)
        {
            return Utilities.TryGetCustomItem(pickup.Serial, out ICustomItem customItem) ? customItem : null;
        }

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
