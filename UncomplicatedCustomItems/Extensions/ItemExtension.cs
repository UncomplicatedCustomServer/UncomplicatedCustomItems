using Exiled.API.Features;
using UncomplicatedCustomItems.API.Features;
using System.Linq;

namespace UncomplicatedCustomItems.Extensions
{
    public static class ItemExtension
    {
        /// <summary>
        /// Try to get the current <see cref="SummonedCustomItem"/> of a <see cref="Player"/> if it's one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="summonedInstance"></param>
        /// <returns>true if the player is currently <see cref="SummonedCustomItem"/></returns>
        public static bool TryGetSummonedInstance(this Player player, out SummonedCustomItem summonedInstance)
        {
            summonedInstance = GetSummonedInstance(player);
            return summonedInstance != null;
        }

        /// <summary>
        /// Get the current <see cref="SummonedCustomItem"/> of a <see cref="Player"/> if it's one.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns>The current <see cref="SummonedCustomItem"/> if the player has one, otherwise <see cref="null"/></returns>
        public static SummonedCustomItem GetSummonedInstance(this Player owner)
        {
            return SummonedCustomItem.Get(owner).FirstOrDefault();
        }

    }
}