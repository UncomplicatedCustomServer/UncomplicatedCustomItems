namespace UncomplicatedCustomItems.API.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Jailbird"/>
    /// </summary>
    public interface IJailbirdData : IData
    {
        public abstract float MeleeDamage { get; set; }

        public abstract float ChargeDamage { get; set; }

        public abstract float FlashDuration { get; set; }

        public abstract float Radius { get; set; }
        
        public abstract int TotalCharges { get; set; }
    }
}
