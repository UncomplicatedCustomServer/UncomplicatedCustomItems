using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class DictionaryExtension
    {
        public static bool TryGetRaw(this Dictionary<object, object> dict, string name, out object value)
        {
            value = null!;
            if (dict == null || name == null)
                return false;

            foreach (var kv in dict)
            {
                if (kv.Key == null)
                    continue;

                if (string.Equals(kv.Key.ToString().Trim(), name, StringComparison.OrdinalIgnoreCase))
                {
                    value = kv.Value;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetValue<T>(this Dictionary<object, object> dict, string name, out T result)
        {
            result = default!;
            if (!dict.TryGetRaw(name, out object raw) || raw == null)
                return false;

            if (raw is T direct)
            {
                result = direct;
                return true;
            }

            Type targetType = typeof(T);
            TypeCode typeCode = Type.GetTypeCode(targetType);

            string rawStr = raw.ToString().Trim();

            try
            {
                if (raw is string)
                {
                    if (targetType.IsEnum)
                    {
                        try
                        {
                            result = (T)Enum.Parse(targetType, rawStr.Replace(" ", ""), ignoreCase: true);
                            return true;
                        }
                        catch (ArgumentException)
                        {
                            LogManager.Warn($"Value for '{name}' is not a valid {targetType.Name} enum: '{rawStr}'");
                            return false;
                        }
                    }

                    switch (typeCode)
                    {
                        case TypeCode.UInt16:
                            if (ushort.TryParse(rawStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var us)) { result = (T)(object)us; return true; }
                            return false;
                        case TypeCode.UInt32:
                            if (uint.TryParse(rawStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui)) { result = (T)(object)ui; return true; }
                            return false;
                        case TypeCode.Int32:
                            if (int.TryParse(rawStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) { result = (T)(object)i; return true; }
                            return false;
                        case TypeCode.Int64:
                            if (long.TryParse(rawStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) { result = (T)(object)l; return true; }
                            return false;
                        case TypeCode.Double:
                            if (double.TryParse(rawStr, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d)) { result = (T)(object)d; return true; }
                            return false;
                        case TypeCode.Boolean:
                            if (bool.TryParse(rawStr, out var b)) { result = (T)(object)b; return true; }
                            return false;
                        case TypeCode.String:
                            result = (T)(object)rawStr;
                            return true;
                    }
                }

                if (raw is IConvertible)
                {
                    switch (typeCode)
                    {
                        case TypeCode.UInt16:
                            result = (T)(object)Convert.ToUInt16(raw, CultureInfo.InvariantCulture);
                            return true;
                        case TypeCode.UInt32:
                            result = (T)(object)Convert.ToUInt32(raw, CultureInfo.InvariantCulture);
                            return true;
                        case TypeCode.Int32:
                            result = (T)(object)Convert.ToInt32(raw, CultureInfo.InvariantCulture);
                            return true;
                        case TypeCode.Int64:
                            result = (T)(object)Convert.ToInt64(raw, CultureInfo.InvariantCulture);
                            return true;
                        case TypeCode.Double:
                            result = (T)(object)Convert.ToDouble(raw, CultureInfo.InvariantCulture);
                            return true;
                        case TypeCode.Boolean:
                            result = (T)(object)Convert.ToBoolean(raw, CultureInfo.InvariantCulture);
                            return true;
                        case TypeCode.String:
                            result = (T)(object)raw.ToString();
                            return true;
                        default:
                            result = (T)Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
                            return true;
                    }
                }

                if (targetType == typeof(List<object>))
                {
                    if (raw is IEnumerable<object> genericList)
                    {
                        result = (T)(object)genericList.ToList();
                        return true;
                    }
                    if (raw is IEnumerable enumList)
                    {
                        result = (T)(object)enumList.Cast<object>().ToList();
                        return true;
                    }
                }
            }
            catch (OverflowException)
            {
                LogManager.Warn($"Value for '{name}' out of range for target type {targetType.Name}: '{rawStr}'");
                return false;
            }
            catch (FormatException)
            {
                LogManager.Warn($"Value for '{name}' has wrong format for target type {targetType.Name}: '{rawStr}'");
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Warn($"Failed to convert '{name}' to {targetType.Name}: {ex.Message}");
                return false;
            }

            return false;
        }

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

        /// <summary>
        /// Adds a range of key-value pairs to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to modify.</param>
        /// <param name="items">A collection of key-value pairs to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the dictionary or items is null.</exception>
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (items is null)
                throw new ArgumentNullException(nameof(items));

            foreach (var kvp in items)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Tries to get the first key that matches the specified value.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="value">The value to find.</param>
        /// <param name="key">When this method returns, contains the key with the specified value, if found; otherwise, the default value for TKey.</param>
        /// <returns>true if a key with the specified value was found; otherwise, false.</returns>
        public static bool TryGetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, out TKey key)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            foreach (var pair in dictionary)
            {
                if (EqualityComparer<TValue>.Default.Equals(pair.Value, value))
                {
                    key = pair.Key;
                    return true;
                }
            }

            key = default!;
            return false;
        }
    }
}