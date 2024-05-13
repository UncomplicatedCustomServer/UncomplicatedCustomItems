namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IPainkillersData
    {
        public abstract float TickHeal { get; set; }

        public abstract float TimeBeforeStartHealing { get; set; }

        public abstract float TickTime { get; set; }

        public abstract float TotalHealing { get; set; }
    }
}
