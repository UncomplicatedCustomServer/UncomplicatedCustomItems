namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface ILifeStealSettings
    {
        public abstract float LifeStealAmount { get; set; }

        public abstract float LifeStealPercentage { get; set; }
    }
}