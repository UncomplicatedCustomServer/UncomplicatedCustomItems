using Exiled.API.Enums;
using Interactables.Interobjects.DoorUtils;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using UnityEngine;

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
        public KeycardLevels Permissions { get; set; } = new KeycardLevels(0, 0, 0);
        public Color TintColor { get; set; } = Color.red;
        public Color PermissionsColor { get; set; } = Color.red;
    }
}
