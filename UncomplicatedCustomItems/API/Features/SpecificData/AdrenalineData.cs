using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Adrenaline"/> <see cref="CustomItem"/>s
    /// </summary>
    public class AdrenalineData : Data, IAdrenalineData
    {
        /// <summary>
        /// Gets or sets the amout of AHP that has to be given to the player
        /// </summary>
        public float Amount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the decay speed of the AHP
        /// </summary>
        public float Decay { get; set; } = 1.2f;

        /// <summary>
        /// Gets or sets the efficacy of the AHP
        /// </summary>
        public float Efficacy { get; set; } = 0.7f;

        /// <summary>
        /// Gets or sets the delay of the decay of the AHP
        /// </summary>
        public float Sustain { get; set; } = 0f;

        /// <summary>
        /// Gets or sets whether the AHP should be persistent
        /// </summary>
        public bool Persistant { get; set; } = false;
    }
}
