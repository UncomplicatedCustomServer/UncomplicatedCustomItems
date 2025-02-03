using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class KeycardData : Data, IKeycardData
    {
        /// <summary>
        /// Gets or sets the <see cref="KeycardPermissions"/> of the KeyCard
        /// </summary>
        public KeycardPermissions Permissions { get; set; } = KeycardPermissions.Checkpoints;
    }
}
