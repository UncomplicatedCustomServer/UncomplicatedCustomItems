﻿using Exiled.API.Features;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.Medikit"/> <see cref="CustomItem"/>s
    /// </summary>
    public class MedikitData : Data, IMedikitData
    {
        /// <summary>
        /// Gets or sets the <see cref="Player.Health"/> that will be regenerated with the use of the medikit
        /// </summary>
        public float Health { get; set; } = 50f;

        /// <summary>
        /// Gets or sets whether the value can surpass the <see cref="Player.MaxHealth"/>
        /// </summary>
        public bool MoreThanMax { get; set; } = false;
    }
}
