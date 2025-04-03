namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Adrenaline"/>
    /// </summary>
    public interface IAdrenalineData
    {
        public abstract float Amount { get; set; }

        public abstract float Decay { get; set; }

        public abstract float Efficacy { get; set; }

        public abstract float Sustain { get; set; }

        public abstract bool Persistant { get; set; }
    }
}
