using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Keycard"/> <see cref="CustomItem"/>s
    /// </summary>
    public class KeycardData : Data, IKeycardData
    {
        /// <summary>
        /// Gets or sets the <see cref="KeycardPermissions"/> of the KeyCard
        /// </summary>
        public KeycardPermissions Permissions { get; set; } = KeycardPermissions.Checkpoints;
    }
}
