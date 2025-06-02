using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class PlayerExtensions
    {
        /// <summary>
        /// Checks whether the player has a keycard of a specific permission.
        /// </summary>
        /// <param name="player"><see cref="Player" /> trying to interact.</param>
        /// <param name="permissions">The permission that's gonna be searched for.</param>
        /// <returns>Whether the player has the required keycard.</returns>
        internal static bool HasKeycardPermission(this Player player, KeycardPermissions permissions)
        {
            return player.Items.Any(item => item is Keycard keycard && keycard.Permissions.HasFlag(permissions));
        }
    }
}