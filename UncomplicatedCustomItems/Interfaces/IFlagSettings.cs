using Exiled.API.Enums;

namespace UncomplicatedCustomItems.Interfaces
{
    #nullable enable
    public interface IFlagSettings
    {
        public abstract string GlowColor { get; set; }

        public abstract float LifeStealAmount { get; set; }

        public abstract float LifeStealPercentage { get; set; }

        public abstract string EffectEvent { get; set; }

        public abstract EffectType Effect {get; set; }

        public abstract byte EffectIntensity { get; set; }

        public abstract float EffectDuration {get; set; }
        
        public abstract string? AudioPath { get; set; }

        public abstract float? AudibleDistance { get; set; }

        public abstract float? SoundVolume { get; set; }
    }
}