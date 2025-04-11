using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class AudioSettings : IAudioSettings
    {
        /// <summary>
        /// Tells the <see cref="AudioApi"/> where the ogg audio file is.
        /// </summary>
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