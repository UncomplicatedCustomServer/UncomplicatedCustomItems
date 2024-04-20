using Exiled.API.Enums;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    internal interface IKeycardData : IData
    {
        public abstract KeycardPermissions Permissions { get; set; }
    }
}
