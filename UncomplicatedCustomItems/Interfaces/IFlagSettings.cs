using System.Collections.Generic;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Interfaces
{
    #nullable enable
    public interface IFlagSettings
    {
        /*
        public abstract string GlowColor { get; set; }
        */
        public abstract List<ItemGlowSettings?> ItemGlowSettings { get; set; }
        /*
        public abstract float LifeStealAmount { get; set; }

        public abstract float LifeStealPercentage { get; set; }
        */
        public abstract List<LifeStealSettings?> LifeStealSettings { get; set; }
        /*
        public abstract string EffectEvent { get; set; }

        public abstract EffectType Effect {get; set; }

        public abstract byte EffectIntensity { get; set; }

        public abstract float EffectDuration {get; set; }
        */

        public abstract List<EffectSettings?> EffectSettings { get; set; }
        /*
        public abstract string? AudioPath { get; set; }

        public abstract float? AudibleDistance { get; set; }

        public abstract float? SoundVolume { get; set; }
        */
        public abstract List<AudioSettings?> AudioSettings { get; set; }
        /*
        public abstract float? DamageRadius { get; set; }
        */
        public abstract List<ExplosiveBulletsSettings?> ExplosiveBulletsSettings { get; set; }
        public abstract List<SpawnItemWhenDetonatedSettings?> SpawnItemWhenDetonatedSettings { get; set; }
        public abstract List<ClusterSettings?> ClusterSettings { get; set; }
        public abstract List<SwitchRoleOnUseSettings?> SwitchRoleOnUseSettings { get; set; }
        public abstract List<DieOnDropSettings?> DieOnDropSettings { get; set; }
        public abstract List<CantDropSettings?> CantDropSettings { get; set; }
    }
}