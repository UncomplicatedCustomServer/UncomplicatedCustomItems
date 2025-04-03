using Exiled.API.Enums;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Keycard"/>
    /// </summary>
    public interface IKeycardData : IData
    {
        public abstract KeycardPermissions Permissions { get; set; }
    }
}
