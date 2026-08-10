using System.Collections.Generic;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UncomplicatedCustomItems.API.YamlObjects;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="Firearm"/>s <see cref="CustomItemType.Weapon"/> <see cref="CustomItem"/>s
    /// </summary>
    public class WeaponData : Data, IWeaponData
    {
        /// <summary>
        /// The damage of the ammo. Negative to heal
        /// </summary>
        public virtual float Damage { get; set; } = 2.75f;

        /// <summary>
        /// The max number of ammunitions
        /// </summary>
        public virtual int MaxAmmo { get; set; } = 150;

        /// <summary>
        /// The max number of ammunitions in the magazine
        /// </summary>
        public virtual int MaxMagazineAmmo { get; set; } = 150;

        public virtual int MaxBarrelAmmo { get; set; } = 16;

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
        /// Gets or sets the how much fast the value drop over the distance.
        /// </summary>
        public virtual float DamageFalloffDistance { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the weapon attachments.
        /// </summary>
        public virtual string Attachments { get; set; } = "DotSight";

        /// <summary>
        /// Gets or sets if the <see cref="CustomItem"/> can damage the friendly team.
        /// </summary>
        public virtual bool EnableFriendlyFire { get; set; } = false;

        public List<ParameterObject> ParameterModifiers { get; set; }
    }
}