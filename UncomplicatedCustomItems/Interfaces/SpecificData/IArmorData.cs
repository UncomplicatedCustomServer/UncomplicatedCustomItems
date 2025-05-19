using System;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Armor"/>
    /// </summary>
    public interface IArmorData : IData
    {
        public abstract int HeadProtection { get; set; }

        public abstract int BodyProtection { get; set; }
        
        [Obsolete("No longer does anything obsoleted by EXILED")]
        public abstract bool RemoveExcessOnDrop { get; set; }

        public abstract float StaminaUseMultiplier { get; set; }
        
        public abstract float StaminaRegenMultiplier { get; set; }
    }
}
