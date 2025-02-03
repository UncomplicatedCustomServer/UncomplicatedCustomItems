using Exiled.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    internal class MedikitData : Data, IMedikitData
    {
        /// <summary>
        /// Gets or sets the health that will be regenerated with the use of the medikit
        /// </summary>
        public float Health { get; set; } = 50f;

        /// <summary>
        /// Gets or sets whether the value can surpass the <see cref="Player.MaxHealth"/>
        /// </summary>
        public bool MoreThanMax { get; set; } = false;
    }
}
