namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
#nullable enable
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Weapon"/>
    /// </summary>
    public interface IWeaponData : IData
    {
        public abstract float Damage { get; set; }

        public abstract int MaxBarrelAmmo { get; set; }

        public abstract int MaxAmmo { get; set; }

        public abstract int MaxMagazineAmmo { get; set; }

        public abstract int AmmoDrain { get; set; }

        public abstract float Penetration { get; set; }

        public abstract float Inaccuracy { get; set; }

        public abstract float DamageFalloffDistance { get; set; }

        public abstract string Attachments { get; set; }

        public abstract bool EnableFriendlyFire { get; set; }
    }

}