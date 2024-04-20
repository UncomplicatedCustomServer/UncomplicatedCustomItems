using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    internal class KeycardData : IKeycardData
    {
        public KeycardPermissions Permissions { get; set; } = KeycardPermissions.Checkpoints;
    }
}
