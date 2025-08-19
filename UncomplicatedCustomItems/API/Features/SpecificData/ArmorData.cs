using System;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Armor"/> <see cref="CustomItem"/>s
    /// </summary>
    public class ArmorData : Data, IArmorData
    {
        /// <summary>
        /// Gets or sets the armor's Head Protection value
        /// </summary>
        public virtual int HeadProtection { get; set; } = 2;

        /// <summary>
        /// Gets or sets the armor's Body Protection value
        /// </summary>
        public virtual int BodyProtection { get; set; } = 3;

        /// <summary>
        /// No longer does anything obsoleted
        /// </summary>
        [Obsolete("No longer does anything obsoleted by EXILED")]
        [Description("No longer does anything obsoleted by EXILED")]
        public virtual bool RemoveExcessOnDrop { get; set; } = true;

        /// <summary>
        /// Gets or sets the stamina that this armor drains
        /// </summary>
        public virtual float StaminaUseMultiplier { get; set; } = 2f;
        
        /// <summary>
        /// Gets or sets the stamina regen multiplier
        /// </summary>
        public virtual float StaminaRegenMultiplier { get; set; } = 2f;
    }
}
