using Exiled.API.Features;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Serializable
{
    public class SerializableCustomWeapon : Interfaces.SerializableThing<CustomWeapon>
    {
        [Description("Name")]
        public override string Name { get; set; }

        [Description("Description")]
        public override string Description { get; set; }

        [Description("Use response")]
        public override int Id { get; set; }

        [Description("Model")]
        public ItemType Model { get; set; }

        [Description("Scale")]
        public Vector3 Scale { get; set; }

        [Description("Weapon info")]
        public WeaponInfo Info { get; set; }

        /// <summary>
        /// Return custom item by serializable 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override CustomWeapon Create(Player player)
        {
            return new CustomWeapon(player, this);
        }
    }
}
