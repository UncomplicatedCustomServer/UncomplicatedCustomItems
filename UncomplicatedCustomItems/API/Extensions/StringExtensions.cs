using System.Globalization;
using System;
using System.Text;
using UnityEngine;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class StringExtensions
    {
        public static string GenerateWithBuffer(this string str, int bufferSize)
        {
            for (int a = str.Length; a < bufferSize; a++)
                str += " ";

            return str;
        }

        /// <summary>
        /// Converts a PascalCase/camelCase string to kebab case.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <param name="separator">The separator to insert. Defaults to '-'.</param>
        /// <param name="separateNumbersAndLetters">When true, also inserts the separator between a digit and a letter (e.g. "SCP127" becomes "scp-127")    .</param>
        /// <returns>The kebab-case string.</returns>
        public static string ToKebabCase(this string str, char separator = '-', bool separateNumbersAndLetters = false)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            StringBuilder sb = new(str.Length + 5);

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (char.IsUpper(c))
                {
                    bool isFirst = sb.Length == 0;
                    char prev = i > 0 ? str[i - 1] : '\0';
                    char next = i + 1 < str.Length ? str[i + 1] : '\0';
                    bool lowerToUpper = !isFirst && (char.IsLower(prev) || char.IsDigit(prev));
                    bool acronymBoundary = !isFirst && char.IsUpper(prev) && char.IsUpper(c) && char.IsLower(next);

                    if (lowerToUpper || acronymBoundary)
                        sb.Append(separator);

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    if (separateNumbersAndLetters && sb.Length > 0 && i > 0)
                    {
                        char prev = str[i - 1];
                        bool letterToDigit = char.IsLetter(prev) && char.IsDigit(c);
                        bool digitToLetter = char.IsDigit(prev) && char.IsLetter(c);

                        if ((letterToDigit || digitToLetter) && sb[sb.Length - 1] != separator)
                            sb.Append(separator);
                    }

                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a comma-separated string into a Vector3.
        /// The string is expected to have exactly 3 components (e.g., "r,g,b"),
        /// where the first three are parsed as floats for the X, Y, and Z values of the Vector3.
        /// </summary>
        /// <param name="inputString">The string to parse. Expected format: "x,y,z"</param>
        /// <param name="parsedVector">The parsed Vector3 if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryParseVector3(this string inputString, out Vector3 parsedVector)
        {
            parsedVector = Vector3.zero;

            if (string.IsNullOrEmpty(inputString))
            {
                return false;
            }

            string[] components = inputString.Split(',');

            if (components.Length >= 3)
            {
                try
                {
                    float x = float.Parse(components[0].Trim(), CultureInfo.InvariantCulture);
                    float y = float.Parse(components[1].Trim(), CultureInfo.InvariantCulture);
                    float z = float.Parse(components[2].Trim(), CultureInfo.InvariantCulture);
                    parsedVector = new Vector3(x, y, z);
                    return true;
                }
                catch (FormatException ex)
                {
                    LogManager.Error($"Failed to parse components to float for TryParseVector3. Input: '{inputString}'. Error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    LogManager.Error($"An unexpected error occurred in TryParseVector3. Input: '{inputString}'. Error: {ex.Message}");
                    return false;
                }
            }
            else
            {
                LogManager.Warn($"Input string '{inputString}' does not have at least 3 components for TryParseVector3.");
                return false;
            }
        }
    }
}
