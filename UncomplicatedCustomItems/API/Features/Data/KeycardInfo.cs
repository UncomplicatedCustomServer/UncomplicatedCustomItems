using Exiled.API.Enums;
using Exiled.API.Features.Items;

namespace UncomplicatedCustomItems.API.Features.Data
{
    public class KeycardInfo : ThingInfo
    {
        public KeycardPermissions Permissions { get; set; }

        public override void Set(Item item)
        {
            if (item is not Keycard keycard)
            {
                return;
            }

            keycard.Permissions = Permissions;
        }
    }
}
