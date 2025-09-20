using System.Collections.Generic;
using InventorySystem.Items.Firearms.Attachments;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomWeapon : BaseCustomItem
    {
        /// <summary>
        /// Gets or sets the damage of the firearm. Negative to heal
        /// </summary>
        public abstract float Damage { get; set; }

        /// <summary>
        /// Gets or sets the max number of ammunitions
        /// </summary>
        public abstract int MaxAmmo { get; set; }

        /// <summary>
        /// Gets or sets the max number of ammunitions in the magazine
        /// </summary>
        public abstract int MaxMagazineAmmo { get; set; }

        /// <summary>
        /// Gets or sets the amount of ammo able to be chambered in the barrel.
        /// </summary>
        public abstract int MaxBarrelAmmo { get; set; }

        /// <summary>
        /// Gets or sets the penetration of the firearm
        /// </summary>
        public abstract float Penetration { get; set; }

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm
        /// </summary>
        public abstract float Inaccuracy { get; set; }

        /// <summary>
        /// Gets or sets the inaccuracy of the firearm while the player is ADS
        /// </summary>
        public abstract float AimingInaccuracy { get; set; }

        /// <summary>
        /// Gets or sets the how much fast the value drop over the distance.
        /// </summary>
        public abstract float DamageFalloffDistance { get; set; }

        /// <summary>
        /// Gets or sets the weapon attachments.
        /// </summary>
        public abstract List<AttachmentName> Attachments { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="CustomItem"/> can damage the friendly team.
        /// </summary>
        public abstract bool EnableFriendlyFire { get; set; }
    }
}