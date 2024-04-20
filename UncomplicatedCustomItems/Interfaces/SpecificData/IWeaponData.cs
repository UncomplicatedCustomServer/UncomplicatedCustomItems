using CameraShaking;
using Exiled.API.Structs;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    internal interface IWeaponData : IData
    {
        public abstract float Damage { get; set; }

        public abstract float FireRate { get; set; }

        public abstract byte MaxAmmo { get; set; }
    }
}
