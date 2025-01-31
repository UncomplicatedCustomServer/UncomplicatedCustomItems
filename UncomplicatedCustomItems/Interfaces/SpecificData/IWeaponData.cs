using CameraShaking;
using Exiled.API.Structs;
using System.Collections.Generic;
using Exiled.CustomItems.API;
using InventorySystem.Items.Firearms.Attachments;
using Exiled.API.Features;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IWeaponData : IData
    {
        public abstract float Damage { get; set; }

        public abstract byte MaxBarrelAmmo { get; set; }

        public abstract byte MaxAmmo { get; set; }

        public abstract byte MaxMagazineAmmo { get; set; }

        public abstract int AmmoDrain { get; set; }
        
        public abstract float Penetration { get; set; }

        public abstract float Inaccuracy { get; set; }
    }
}
