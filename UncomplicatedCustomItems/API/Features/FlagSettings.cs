using Exiled.API.Enums;
using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class FlagSettings : IFlagSettings
    {
        /// <summary>
        /// Sets the glow color of the item if it has the ItemGlow custom flag.
        /// </summary>
        [Description("Sets the glow color of the item if it has the ItemGlow custom flag.")]
        public string GlowColor { get; set; } = "#00FF00";

        /// <summary>
        /// Sets the life steal amount of the item if it has the LifeSteal custom flag.
        /// </summary>
        [Description("Sets the life steal amount of the item if it has the LifeSteal custom flag.")]
        public float LifeStealAmount { get; set; } = 8f;

        /// <summary>
        /// Sets the percentage of health regenerated if the item has the HalfLifeSteal custom flag.
        /// </summary>
        [Description("Sets the percentage of health regenerated if the item has the HalfLifeSteal custom flag. HealedAmount = Amount * Percentage")]
        public float LifeStealPercentage { get; set; } = 0.5f;

        /// <summary>
        /// Sets the effect event that should be triggered by the custom item. 
        /// This should be modified based on the associated flag.
        /// </summary>
        [Description("Sets the effect event that should be triggered by the custom item. Modify this based on the associated flag.")]
        public string EffectEvent { get; set; } = "PickedUpEffect";

        /// <summary>
        /// Sets the effect that the custom item will apply.
        /// </summary>
        [Description("Sets the effect that the custom item will apply.")]
        public EffectType Effect { get; set; } = new();

        /// <summary>
        /// Sets the intensity of the effect applied by the custom item.
        /// </summary>
        [Description("Sets the intensity of the effect applied by the custom item.")]
        public byte EffectIntensity { get; set; } = 1;

        /// <summary>
        /// Sets the duration of the effect applied by the custom item.
        /// </summary>
        [Description("Sets the duration of the effect applied by the custom item.")]
        public float EffectDuration { get; set; } = -1f;

#nullable enable
        /// <summary>
        /// Tells the AudioAPI where the ogg audio file is.
        /// </summary>
        [Description("Tells the AudioAPI where the ogg audio file is.")]
        public string? AudioPath { get; set; } = "";

        /// <summary>
        /// Sets the distance that the audio will be heard for.
        /// </summary>
        [Description("Sets the distance that the audio will be heard for.")]
        public float? AudibleDistance { get; set; } = 10f;

        /// <summary>
        /// Sets the volume of the audio.
        /// </summary>
        [Description("Sets the volume percent of the audio.")]
        public float? SoundVolume { get; set; } = 10f;

    }
}
