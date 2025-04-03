using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Painkillers"/> <see cref="CustomItem"/>s
    /// </summary>
    public class PainkillersData : Data, IPainkillersData
    {
        /// <summary>
        /// Gets or sets the heal that will be granted to the player every <see cref="TickTime"/>
        /// </summary>
        public float TickHeal { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the time that will be waited before healing the player
        /// </summary>
        public float TimeBeforeStartHealing { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the time between the <see cref="TickHeal"/>
        /// </summary>
        public float TickTime { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the total heal that can be granted
        /// </summary>
        public float TotalHealing { get; set; } = 25f;
    }
}
