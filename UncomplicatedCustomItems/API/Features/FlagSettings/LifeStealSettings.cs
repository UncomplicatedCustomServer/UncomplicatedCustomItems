using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class LifeStealSettings : ILifeStealSettings
    {
        /// <summary>
        /// Sets the life steal amount of the <see cref="ICustomItem"/> if it has the LifeSteal <see cref="ICustomModule"/>.
        /// </summary>
        public float LifeStealAmount { get; set; } = 8f;

        /// <summary>
        /// Sets the percentage of health regenerated if the <see cref="ICustomItem"/> has the HalfLifeSteal <see cref="ICustomModule"/>.
        /// </summary>
        [Description("Sets the percentage of health regenerated if the item has the HalfLifeSteal custom flag. HealedAmount = Amount * Percentage")]
        public float LifeStealPercentage { get; set; } = 0.5f;
    }
}