using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.MicroHID"/> <see cref="CustomItem"/>s
    /// </summary>
    public class MicroHIDData : Data, IMicroHIDData
    {
        /// <summary>
        /// Gets or sets the amount of damage done to a player/>
        /// </summary>
        public virtual float Damage { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the amount of energy in the MicroHID/>
        /// </summary>
        public virtual float Energy { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets if the MicroHID can explode/>
        /// </summary>
        public virtual bool CanExplode { get; set; } = true;
    }
}
