using Exiled.API.Features;
using Exiled.API.Features.Items;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;
using static PlayerArms;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomArmor : CustomThing
    {
        public CustomArmor(Player player, ArmorInfo armorInfo, SerializableCustomArmor serializableCustomItem) : base(player, armorInfo, serializableCustomItem) { }
    }
}
