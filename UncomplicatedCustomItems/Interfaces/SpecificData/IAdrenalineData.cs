namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    internal interface IAdrenalineData : IData
    {
        public abstract float Amount { get; set; }

        public abstract float Decay { get; set; }

        public abstract float Efficacy { get; set; }

        public abstract float Sustain { get; set; }

        public abstract bool Persistant { get; set; }
    }
}
