using Exiled.API.Features.Items;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Extensions
{
    /// <summary>
    /// Contains all the item extensions for UCI.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="Item"/> is a custom item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is a custom item; otherwise, <c>false</c>.</returns>
        public static bool IsCustomItem(this Item item) => Utilities.IsCustomItem(item.Serial);

        /// <summary>
        /// Determines whether the specified <see cref="Item"/> is a summoned custom item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is a summoned custom item; otherwise, <c>false</c>.</returns>
        public static bool IsSummonedCustomItem(this Item item) => Utilities.IsSummonedCustomItem(item.Serial);

        /// <summary>
        /// Attempts to retrieve the <see cref="SummonedCustomItem"/> associated with the specified <see cref="Item"/>.
        /// </summary>
        /// <param name="item">The item to query.</param>
        /// <returns>
        /// The corresponding <see cref="SummonedCustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static SummonedCustomItem TryGetSummonedCustomItem(this Item item)
        {
            return Utilities.TryGetSummonedCustomItem(item.Serial, out SummonedCustomItem customItem) ? customItem : null;
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="ICustomItem"/> associated with the specified <see cref="Item"/>.
        /// </summary>
        /// <param name="item">The item to query.</param>
        /// <returns>
        /// The corresponding <see cref="ICustomItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public static ICustomItem TryGetCustomItem(this Item item)
        {
            return Utilities.TryGetCustomItem(item.Serial, out ICustomItem customItem) ? customItem : null;
        }
        
        /// <summary>
        /// Compares two <see cref="Item"/> instances to determine if they refer to the same custom item definition.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>
        /// <c>true</c> if both items refer to the same custom item definition; otherwise, <c>false</c>.
        /// </returns>
        public static bool CompareCustomItems(this Item item1, Item item2) => TryGetCustomItem(item1) == TryGetCustomItem(item2);

        /// <summary>
        /// Compares two <see cref="Item"/> instances to determine if they are the same summoned custom item.
        /// </summary>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare.</param>
        /// <returns>
        /// <c>true</c> if both items refer to the same summoned custom item instance; otherwise, <c>false</c>.
        /// </returns>
        public static bool CompareSummonedCustomItems(this Item item1, Item item2) => TryGetSummonedCustomItem(item1) == TryGetSummonedCustomItem(item2);
    }
}