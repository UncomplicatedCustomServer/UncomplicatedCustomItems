namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IWeaponData : IData
    {
        public abstract float Damage { get; set; }

        public abstract float FireRate { get; set; }

        public abstract byte MaxAmmo { get; set; }
    }
}
