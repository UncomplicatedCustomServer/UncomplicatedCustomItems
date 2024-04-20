namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    internal interface IArmorData : IData
    {
        public abstract int HeadProtection { get; set; }

        public abstract int BodyProtection { get; set; }

        public abstract bool RemoveExcessOnDrop { get; set; }

        public abstract float StaminaUseMultiplier { get; set; }
    }
}
