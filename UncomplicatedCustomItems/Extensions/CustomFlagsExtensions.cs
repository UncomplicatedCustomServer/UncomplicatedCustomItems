using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Extensions
{
    /// <summary>
    /// Class for handling new <see cref="CustomFlags"/> added by other plugins.
    /// </summary>
    public static class CustomFlagsExtensions
    {
        /// <summary>
        /// Checks if the provided <see cref="CustomFlags"/> instance contains the specified custom flag value (represented as an integer).
        /// This is useful for checking flags that might be defined numerically by other plugins and not present in the base <see cref="CustomFlags"/> enum.
        /// </summary>
        /// <param name="flags">The <see cref="CustomFlags"/> instance to check.</param>
        /// <param name="customFlagValue">The integer value representing the custom flag to check for.</param>
        /// <returns><see langword="true"/> if the custom flag represented by <paramref name="customFlagValue"/> is set within the <paramref name="flags"/>; otherwise, <see langword="false"/>.</returns>
        public static bool HasCustomFlag(this CustomFlags flags, int customFlagValue)
        {
            return ((int)flags & customFlagValue) == customFlagValue;
        }

        /// <summary>
        /// Adds the specified custom flag value (represented as an integer) to the <see cref="CustomFlags"/> instance and returns the resulting combined flags.
        /// This allows adding flags defined numerically by other plugins.
        /// </summary>
        /// <param name="flags">The original <see cref="CustomFlags"/> instance.</param>
        /// <param name="customFlagValue">The integer value representing the custom flag to add.</param>
        /// <returns>A new <see cref="CustomFlags"/> value that includes the original <paramref name="flags"/> plus the flag represented by <paramref name="customFlagValue"/>.</returns>
        public static CustomFlags AddCustomFlag(this CustomFlags flags, int customFlagValue)
        {
            return flags | (CustomFlags)customFlagValue;
        }
    }
}