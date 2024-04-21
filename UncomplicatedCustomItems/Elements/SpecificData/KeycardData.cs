using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class KeycardData : Data, IKeycardData
    {
        public KeycardPermissions Permissions { get; set; } = KeycardPermissions.Checkpoints;
    }
}
