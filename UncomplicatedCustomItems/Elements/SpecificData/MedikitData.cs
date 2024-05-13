using Exiled.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    internal class MedikitData : Data, IMedikitData
    {
        /// <summary>
        /// The health that will be regenerated with the use of the medikit
        /// </summary>
        public float Health { get; set; } = 50f;

        /// <summary>
        /// If <see cref="true"/> the value can surpass the <see cref="Player.MaxHealth"/>
        /// </summary>
        public bool MoreThanMax { get; set; } = false;
    }
}
