using Exiled.API.Enums;
using Interactables.Interobjects.DoorUtils;
using UnityEngine;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Keycard"/>
    /// </summary>
    public interface IKeycardData : IData
    {
        public abstract Color TintColor { get; set; }
        public abstract KeycardLevels Permissions { get; set; }
        public abstract Color PermissionsColor { get; set; }
    }
}
