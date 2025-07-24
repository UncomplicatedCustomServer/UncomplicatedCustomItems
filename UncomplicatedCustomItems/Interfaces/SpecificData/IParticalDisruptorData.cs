namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.ParticalDisruptor"/>
    /// </summary>
    public interface IParticalDisruptorData
    {
        public abstract float Damage { get; set; }

        public abstract int Ammo { get; set; }

        public abstract int LaserAmount { get; set; }

        public abstract float Penetration { get; set; }

        public abstract float Inaccuracy { get; set; }

        public float AimingInaccuracy { get; set; }

        public abstract bool EnableFriendlyFire { get; set; }

        public abstract string FiringState { get; set; }
    }
}
