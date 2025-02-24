using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class ArmorData : Data, IArmorData
    {
        /// <summary>
        /// Gets or sets the armor's Head Protection value
        /// </summary>
        public int HeadProtection { get; set; } = 2;

        /// <summary>
        /// Gets or sets the armor's Body Protection value
        /// </summary>
        public int BodyProtection { get; set; } = 3;

        /// <summary>
        /// Gets or sets whether the excess ammo should be dropped when the armor is dropped
        /// </summary>
        public bool RemoveExcessOnDrop { get; set; } = true;

        /// <summary>
        /// Gets or sets the stamina that this armor drains
        /// </summary>
        public float StaminaUseMultiplier { get; set; } = 2f;
        
        /// <summary>
        /// Gets or sets the stamina regen multiplier
        /// </summary>
        public float StaminaRegenMultiplier { get; set; } = 2f;
    }
}
