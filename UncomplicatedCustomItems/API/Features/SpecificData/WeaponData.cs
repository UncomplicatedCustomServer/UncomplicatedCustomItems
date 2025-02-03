using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class WeaponData : Data, IWeaponData
    {
        /// <summary>
        /// Gets or sets the damage of the ammo. Negative to heal
        /// </summary>
        public float Damage { get; set; } = 2.75f;

        /// <summary>
        /// Gets or sets the fire rate, lower is faster.<br></br>
        /// This field will be effective only if the Firearm is automatic
        /// </summary>
        public float FireRate { get; set; } = 0.35f;

        /// <summary>
        /// Gets or sets the maximum number of ammunitions
        /// </summary>
        public byte MaxAmmo { get; set; } = 20;
    }
}
