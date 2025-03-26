namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IJailbirdData : IData
    {
        public float MeleeDamage { get; set; }

        public float ChargeDamage { get; set; }

        public float FlashDuration { get; set; }

        public float Radius { get; set; }
        
        public int TotalCharges { get; set; }
    }
}
