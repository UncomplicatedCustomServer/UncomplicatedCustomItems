using CustomPlayerEffects;
using System;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface IEffectSettings
    {
        public abstract string EffectEvent { get; set; }

        public abstract string Effect {get; set; }

        public abstract byte EffectIntensity { get; set; }

        public abstract float EffectDuration {get; set; }
        public abstract bool? AddDurationIfActive { get; set; }
    }
}