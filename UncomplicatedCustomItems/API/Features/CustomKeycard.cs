using Exiled.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomKeycard : CustomThing
    {
        public CustomKeycard(Player player, KeycardInfo keycardInfo, SerializableCustomKeycard serializableCustomKeycard) : base(player, keycardInfo, serializableCustomKeycard) { }
    }
}
