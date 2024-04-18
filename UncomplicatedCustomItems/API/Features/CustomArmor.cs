using Exiled.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomArmor : CustomThing
    {
        public CustomArmor(Player player, ArmorInfo armorInfo, SerializableCustomArmor serializableCustomItem) : base(player, armorInfo, serializableCustomItem) { }
    }
}
