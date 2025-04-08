using System.ComponentModel;
using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class EffectSettings : IEffectSettings
    {
        /// <summary>
        /// Sets the effect event that should be triggered by the <see cref="ICustomItem"/>. 
        /// This should be modified based on the associated <see cref="ICustomModule"/>.
        /// </summary>
        public string EffectEvent { get; set; } = "PickedUpEffect";

        /// <summary>
        /// Sets the <see cref="EffectType"/> that the <see cref="ICustomItem"/> will apply.
        /// </summary>
        [Description("Sets the effect that the custom item will apply.")]
        public EffectType Effect { get; set; } = new();

        /// <summary>
        /// Sets the intensity of the <see cref="EffectType"/> applied by the <see cref="ICustomItem"/>.
        /// </summary>
        [Description("Sets the intensity of the effect applied by the custom item.")]
        public byte EffectIntensity { get; set; } = 1;

        /// <summary>
        /// Sets the duration of the <see cref="EffectType"/> applied by the <see cref="ICustomItem"/>.
        /// </summary>
        [Description("Sets the duration of the effect applied by the custom item.")]
        public float EffectDuration { get; set; } = -1f;
    }
}