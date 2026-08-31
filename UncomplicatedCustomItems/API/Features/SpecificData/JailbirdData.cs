using InventorySystem.Items.Jailbird;
namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Jailbird"/> <see cref="CustomItem"/>s
    /// </summary>
    public class JailbirdData : Data
    {
        /// <summary>
        /// Gets or sets the amount of damage dealt with a Jailbird melee hit.
        /// </summary>
        public virtual float MeleeDamage { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the amount of damage dealt with a Jailbird charge hit.
        /// </summary>
        public virtual float ChargeDamage { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the amount of time in seconds that the <see cref="CustomPlayerEffects.Flashed"/> effect will be applied on being hit.
        /// </summary>
        public virtual float FlashDuration { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the radius of the Jailbird's hit register.
        /// </summary>
        public virtual float Radius { get; set; } = 3f;

        public bool AllowWearStateChanges { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="JailbirdWearState"/> of the <see cref="JailbirdItem"/>
        /// </summary>
        public JailbirdWearState WearState { get; set; }

        internal float TotalHits { get; set; }
        internal float TotalCharges { get; set; }
        internal float TotalDamage { get; set; }
        internal float TotalHitDamage { get; set; }
        internal float TotalChargeDamage { get; set; }
    }
}
