using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomWeapon : CustomThing
    {
        public CustomWeapon(Player player, WeaponInfo weaponInfo, SerializableCustomWeapon serializableCustomWeapon) : base(player, weaponInfo, serializableCustomWeapon) { }
    }
}
