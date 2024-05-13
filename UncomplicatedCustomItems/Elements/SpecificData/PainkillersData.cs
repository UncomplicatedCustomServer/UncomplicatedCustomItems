using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class PainkillersData : Data, IPainkillersData
    {
        /// <summary>
        /// The heal that will be granted to the player every <see cref="TickTime"/>
        /// </summary>
        public float TickHeal { get; set; } = 2f;

        /// <summary>
        /// The time that will be waited before healing the player
        /// </summary>
        public float TimeBeforeStartHealing { get; set; } = 5f;

        /// <summary>
        /// The time between the <see cref="TickHeal"/>
        /// </summary>
        public float TickTime { get; set; } = 0.5f;

        /// <summary>
        /// The total heal that can be granted
        /// </summary>
        public float TotalHealing { get; set; } = 25f;
    }
}
