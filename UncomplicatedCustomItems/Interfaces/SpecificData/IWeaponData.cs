using Exiled.API.Enums;


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

        public abstract float DamageFalloffDistance { get; set; }
    }

}