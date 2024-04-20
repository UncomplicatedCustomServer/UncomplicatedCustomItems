using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class WeaponData : IWeaponData
    {
        /// <summary>
        /// The damage of the ammo. Negative to heal
        /// </summary>
        public float Damage { get; set; } = 2.75f;

        /// <summary>
        /// The fire rate, lower is faster
        /// </summary>
        public float FireRate { get; set; } = 0.35f;

        /// <summary>
        /// The max number of ammunitions
        /// </summary>
        public byte MaxAmmo { get; set; } = 20;
    }
}
