namespace UncomplicatedCustomItems.API.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.ParticalDisruptor"/>
    /// </summary>
    public interface IParticleDisruptorData
    {
        public abstract float BurstDamage { get; set; }

        public abstract float ChargeDamage { get; set; }

        public abstract float Penetration { get; set; }

        public abstract float Inaccuracy { get; set; }

        public float AimingInaccuracy { get; set; }

        public abstract bool EnableFriendlyFire { get; set; }
    }
}
