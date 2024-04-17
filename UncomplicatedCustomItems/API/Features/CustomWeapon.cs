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
        public CustomWeapon(Player player, SerializableCustomWeapon serializableCustomWeapon)
        {
            Name = serializableCustomWeapon.Name;
            Description = serializableCustomWeapon.Description;
            Info = serializableCustomWeapon.Info;

            Player = player;

            _serializable = serializableCustomWeapon;
        }

        public override string Name { get; }

        public override string Description { get; }

        public override Item Item { get; protected set; }

        public Player Player { get; }

        public WeaponInfo Info { get; }

        private readonly SerializableCustomWeapon _serializable;

        public void Set(Firearm firearm)
        {
            firearm.FireRate = Info.FireRate;
            firearm.MaxAmmo = Info.MaxAmmo;
            firearm.Ammo = firearm.MaxAmmo;
        }

        /// <summary>
        /// Spawn custom item in hand
        /// </summary>
        public override void Spawn()
        {
            Item = Item.Create(_serializable.Model, Player);
            Item.Scale = _serializable.Scale;
            
            if (Item is Firearm firearm)
            {
                Set(firearm);
            }

            Player.CurrentItem = Item;

            Plugin.API.Add(this);
        }
    }
}
