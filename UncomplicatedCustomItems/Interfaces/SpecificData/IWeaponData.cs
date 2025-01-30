using CameraShaking;
using Exiled.API.Structs;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IWeaponData : IData
    {
        public abstract float Damage { get; set; }

        public abstract byte MaxBarrelAmmo { get; set; }

        public abstract byte MaxAmmo { get; set; }

        public abstract byte MaxMagazineAmmo { get; set; }

        public int AmmoDrain { get; set; }
    }
}
