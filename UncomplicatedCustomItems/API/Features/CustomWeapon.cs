using Exiled.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomWeapon : CustomThing
    {
        public CustomWeapon(Player player, WeaponInfo weaponInfo, SerializableCustomWeapon serializableCustomWeapon) : base(player, weaponInfo, serializableCustomWeapon) { }
    }
}
