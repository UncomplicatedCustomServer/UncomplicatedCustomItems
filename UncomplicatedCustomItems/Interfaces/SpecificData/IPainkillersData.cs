namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Painkillers"/>
    /// </summary>
    public interface IPainkillersData
    {
        public abstract float TickHeal { get; set; }

        public abstract float TimeBeforeStartHealing { get; set; }

        public abstract float TickTime { get; set; }

        public abstract float TotalHealing { get; set; }
    }
}
