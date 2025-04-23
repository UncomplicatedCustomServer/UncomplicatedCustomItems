namespace UncomplicatedCustomItems.Interfaces.FlagSettings
{
    internal interface IClusterSettings
    {
        public abstract ItemType ItemToSpawn { get; set; }
        public abstract int? AmountToSpawn { get; set; }
        public abstract float? ScpDamageMultiplier { get; set; }
        public abstract float? FuseTime { get; set; }
    }
}
