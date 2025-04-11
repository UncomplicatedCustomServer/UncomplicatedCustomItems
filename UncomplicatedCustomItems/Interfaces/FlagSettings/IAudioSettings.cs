namespace UncomplicatedCustomItems.Interfaces
{
    public interface IAudioSettings
    {
        public abstract string? AudioPath { get; set; }

        public abstract float? AudibleDistance { get; set; }

        public abstract float? SoundVolume { get; set; }
    }
}