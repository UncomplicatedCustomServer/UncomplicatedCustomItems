using System.Collections.Generic;
using Exiled.API.Enums;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface IEffectSettings
    {
        public abstract string EffectEvent { get; set; }

        public abstract EffectType Effect {get; set; }

        public abstract byte EffectIntensity { get; set; }

        public abstract float EffectDuration {get; set; }
        public abstract bool? AddDurationIfActive { get; set; }
    }
}