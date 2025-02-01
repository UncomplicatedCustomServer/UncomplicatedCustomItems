using UncomplicatedCustomItems.Elements.SpecificData;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IArmorData : IData
    {
        public abstract int HeadProtection { get; set; }

        public abstract int VestEfficacy { get; set; }

        public abstract bool RemoveExcessOnDrop { get; set; }

        public abstract float StaminaUseMultiplier { get; set; }

        public abstract float StaminaRegenMultiplier { get; set; }
    }
}
