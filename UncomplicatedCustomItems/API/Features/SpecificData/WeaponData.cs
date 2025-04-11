using UncomplicatedCustomItems.Interfaces.SpecificData;
using Exiled.API.Features.Items;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
#nullable enable
    /// <summary>
    /// The data associated with <see cref="Firearm"/>s <see cref="CustomItemType.Weapon"/> <see cref="CustomItem"/>s
    /// </summary>
    public class WeaponData : Data, IWeaponData
    {
        /// <summary>
        /// The damage of the ammo. Negative to heal
        /// </summary>
        public float Damage { get; set; } = 2.75f;

        /// <summary>
        /// The max number of ammunitions in the barrel. Shotgun like effect at higher numbers
        /// </summary>
        public int MaxBarrelAmmo { get; set; } = 10;

        /// <summary>
        /// The max number of ammunitions
        /// </summary>
        public int MaxAmmo { get; set; } = 150;

        /// <summary>
        /// The max number of ammunitions in the magazine
        /// </summary>
        public int MaxMagazineAmmo { get; set; } = 150;

        /// <summary>
        /// The amount of ammunitions drained per shot
        /// </summary>
        public int AmmoDrain { get; set; } = 1;

        /// <summary>
        /// Gets or sets the penetration of the firearm
        /// </summary>
        public float Penetration { get; set; } = 1.24f;

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm
        /// </summary>
        public float Inaccuracy { get; set; } = 1.24f;

        /// <summary>
        /// Gets or sets the how much fast the value drop over the distance.
        /// </summary>
        public float DamageFalloffDistance { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the weapon attachments.
        /// </summary>
        public string Attachments { get; set; } = "DotScope";

        /// <summary>
        /// Gets or sets if the <see cref="CustomItem"/> can damage the friendly team.
        /// </summary>
        public bool EnableFriendlyFire { get; set; } = false;
    }
}