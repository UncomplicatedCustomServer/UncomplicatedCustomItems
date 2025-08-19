using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.ParticalDisruptor"/> <see cref="CustomItem"/>s
    /// </summary>
    public class ParticleDisruptorData : Data, IParticleDisruptorData
    {
        /// <summary>
        /// The damage of the ammo. Negative to heal
        /// </summary>
        public virtual float BurstDamage { get; set; } = 2.75f;

        public virtual float ChargeDamage { get; set; } = 2.75f;

        /// <summary>
        /// Gets or sets the penetration of the firearm
        /// </summary>
        public virtual float Penetration { get; set; } = 1.24f;

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm
        /// </summary>
        public virtual float Inaccuracy { get; set; } = 1.24f;

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm while the player is ADS
        /// </summary>
        public virtual float AimingInaccuracy { get; set; } = 1.24f;

        /// <summary>
        /// Gets or sets if the <see cref="CustomItem"/> can damage the friendly team.
        /// </summary>
        public virtual bool EnableFriendlyFire { get; set; } = false;
    }
}
