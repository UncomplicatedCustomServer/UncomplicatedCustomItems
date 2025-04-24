using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.Extensions
{
    /// <summary>
    /// Class for handling new <see cref="CustomFlags"/> added by other plugins.
    /// </summary>
    public static class CustomFlagsExtensions
    {
        /// <summary>
        /// Checks if the provided <see cref="CustomFlags"/> instance contains the specified CustomFlag value (represented as a <see langword="long"/>).
        /// This is useful for checking flags that might be defined numerically by other plugins and not present in the base <see cref="CustomFlags"/> enum.
        /// </summary>
        /// <param name="flags">The <see cref="CustomFlags"/> instance to check.</param>
        /// <param name="customFlagValue">The <see langword="long"/> value representing the CustomFlag to check for.</param>
        /// <returns><see langword="true"/> if the CustomFlag represented by <paramref name="customFlagValue"/> is set within the <paramref name="flags"/>; otherwise, <see langword="false"/>.</returns>
        public static bool HasCustomFlag(this CustomFlags flags, long customFlagValue)
        {
            return ((long)flags & customFlagValue) == customFlagValue;
        }

        /// <summary>
        /// Adds the specified CustomFlag value (represented as a <see langword="long"/>) to the <see cref="CustomFlags"/> instance and returns the resulting combined flags.
        /// This allows adding flags defined numerically by other plugins.
        /// </summary>
        /// <param name="flags">The original <see cref="CustomFlags"/> instance.</param>
        /// <param name="customFlagValue">The <see langword="long"/> value representing the CustomFlag to add.</param>
        /// <returns>A new <see cref="CustomFlags"/> value that includes the original <paramref name="flags"/> plus the flag represented by <paramref name="customFlagValue"/>.</returns>
        public static CustomFlags AddCustomFlag(this CustomFlags flags, long customFlagValue)
        {
            return flags | (CustomFlags)customFlagValue;
        }
    }
}