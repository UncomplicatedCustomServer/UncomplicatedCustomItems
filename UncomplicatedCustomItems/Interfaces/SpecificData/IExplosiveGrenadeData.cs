namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.ExplosiveGrenade"/>
    /// </summary>
    public interface IExplosiveGrenadeData : IData
    {
        public abstract float MaxRadius { get; set; }

        public abstract float ScpDamageMultiplier { get; set; }

        public abstract float BurnDuration { get; set; }

        public abstract float DeafenDuration { get; set; }

        public abstract float ConcussDuration { get; set; }

        public abstract float FuseTime { get; set; }

        public abstract float PinPullTime { get; set; }

        public abstract bool Repickable { get; set; }
    }
}
