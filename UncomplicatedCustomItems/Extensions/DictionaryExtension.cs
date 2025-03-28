using System;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Extensions
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Attempts to add a key-value pair to the dictionary. If the key already exists, updates its value.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to modify.</param>
        /// <param name="Key">The key to add or update.</param>
        /// <param name="value">The value to associate with the key.</param>
        /// <exception cref="ArgumentNullException">Thrown if the dictionary is null.</exception>
        public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey Key, TValue value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(Key))
                dictionary[Key] = value;
            else
                dictionary.Add(Key, value);
        }

        /// <summary>
        /// Attempts to retrieve an element from the dictionary. If the key does not exist, returns a default value.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="ifNot">The default value to return if the key is not found.</param>
        /// <returns>The value associated with the key, or the default value if the key does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the dictionary is null.</exception>
        public static TValue TryGetElement<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue ifNot)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(key))
                return dictionary[key];

            return ifNot;
        }

        /// <summary>
        /// Attempts to remove a key from the dictionary. If the key does not exist, does nothing.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to modify.</param>
        /// <param name="key">The key to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if the dictionary is null.</exception>
        public static void TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(key))
                dictionary.Remove(key);
        }

        /// <summary>
        /// Returns a string representation of the dictionary, including its type and contents.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to convert to a string.</param>
        /// <returns>A formatted string representation of the dictionary.</returns>
        public static string ToRealString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary is null)
                return string.Empty;

            string Data = $"[{dictionary.GetType().FullName}] Dictionary<{dictionary.GetType().GetGenericArguments()[0].FullName}, {dictionary.GetType().GetGenericArguments()[1].FullName}> ({dictionary.Count}) [\n";

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                Data += $"{kvp.Key}: {kvp.Value},\n";

            Data += "];";

            return Data;
        }
    }
}