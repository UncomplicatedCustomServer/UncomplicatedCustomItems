using System;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class ListExtension
    {
        /// <summary>
        /// Attempts to add an item to the list if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to modify.</param>
        /// <param name="item">The item to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the list is null.</exception>
        public static void TryAdd<T>(this List<T> list, T item)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (!list.Contains(item))
                list.Add(item);
        }

        /// <summary>
        /// Converts the list to a formatted string representation.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to convert.</param>
        /// <returns>A string representation of the list.</returns>
        public static string ToRealString<T>(this List<T> list)
        {
            if (list is null)
                return "null value";

            string Data = $"[{list.GetType().FullName}] List<{list.GetType().GetGenericArguments()[0].FullName}> ({list.Count}) [\n";

            foreach (T element in list)
                Data += $"{element},\n";

            Data += "];";

            return Data;
        }
    }
}