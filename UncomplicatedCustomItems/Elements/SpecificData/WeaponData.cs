using CameraShaking;
using UncomplicatedCustomItems.Elements.SpecificData;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using InventorySystem.Items.Firearms.Attachments;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class WeaponData : Data, IWeaponData
    {
        /// <summary>
        /// The damage of the ammo. Negative to heal
        /// </summary>
        public float Damage { get; set; } = 2.75f;

        /// <summary>
        /// The max number of ammunitions in the barrel. Shotgun like effect at higher numbers
        /// </summary>
        public byte MaxBarrelAmmo { get; set; } = 10;

        /// <summary>
        /// The max number of ammunitions
        /// </summary>
        public byte MaxAmmo { get; set; } = 150;

        /// <summary>
        /// The max number of ammunitions in the magazine
        /// </summary>
        public byte MaxMagazineAmmo { get; set; } = 150;

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

        public virtual bool FriendlyFire { get; set; } = false;

    }
}