namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomArmor : APICustomItem
    {
        /// <summary>
        /// Gets or sets the armor's Head Protection value
        /// </summary>
        public abstract int HeadProtection { get; set; }

        /// <summary>
        /// Gets or sets the armor's Body Protection value
        /// </summary>
        public abstract int BodyProtection { get; set; }

        /// <summary>
        /// Gets or sets the stamina that this armor drains
        /// </summary>
        public abstract float StaminaUseMultiplier { get; set; }
        
        /// <summary>
        /// Gets or sets the stamina regen multiplier
        /// </summary>
        public abstract float StaminaRegenMultiplier { get; set; }
    }
}