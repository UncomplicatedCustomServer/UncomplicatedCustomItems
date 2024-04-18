using Exiled.API.Features;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Serializable
{
    public class SerializableCustomWeapon : SerializableThing
    {
        [Description("Weapon info")]
        public WeaponInfo Info { get; set; }

        /// <summary>
        /// Return custom item by serializable 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override CustomThing Create(Player player) => new CustomWeapon(player, Info, this);
    }
}
