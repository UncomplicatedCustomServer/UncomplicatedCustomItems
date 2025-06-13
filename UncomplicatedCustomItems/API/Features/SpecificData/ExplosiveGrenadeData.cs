using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using Exiled.API.Enums;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.ExplosiveGrenade"/> <see cref="CustomItem"/>s
    /// </summary>
    public class ExplosiveGrenadeData : Data, IExplosiveGrenadeData
    {
        /// <summary>
        /// Gets or sets the maximum radius of the grenade.
        /// </summary>
        public virtual float MaxRadius { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the multiplier for damage against <see cref="Side.Scp"/> players.
        /// </summary>
        public virtual float ScpDamageMultiplier { get; set; } = 1.5f;

        /// <summary>
        /// Gets or sets how long the <see cref="EffectType.Burned"/> effect will last.
        /// </summary>
        public virtual float BurnDuration { get; set; } = 10f;

        /// <summary>
        /// Gets or sets how long the <see cref="EffectType.Deafened"/> effect will last.
        /// </summary>
        public virtual float DeafenDuration { get; set; } = 15f;

        /// <summary>
        /// Gets or sets how long the <see cref="EffectType.Concussed"/> effect will last.
        /// </summary>
        public virtual float ConcussDuration { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        public virtual float FuseTime { get; set; } = 3f;

        /// <summary>
        /// The time to pull out the pin
        /// </summary>
        public virtual float PinPullTime { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets a value indicating whether players can pickup grenade after throw.
        /// </summary>
        public virtual bool Repickable { get; set; } = false;
    }
}
