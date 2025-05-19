using System.Collections.Generic;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.Interfaces
{
    #nullable enable
    public interface IFlagSettings
    {
        public abstract List<ItemGlowSettings?> ItemGlowSettings { get; set; }
        public abstract List<LifeStealSettings?> LifeStealSettings { get; set; }
        public abstract List<EffectSettings?> EffectSettings { get; set; }
        public abstract List<AudioSettings?> AudioSettings { get; set; }
        public abstract List<ExplosiveBulletsSettings?> ExplosiveBulletsSettings { get; set; }
        public abstract List<SpawnItemWhenDetonatedSettings?> SpawnItemWhenDetonatedSettings { get; set; }
        public abstract List<ClusterSettings?> ClusterSettings { get; set; }
        public abstract List<SwitchRoleOnUseSettings?> SwitchRoleOnUseSettings { get; set; }
        public abstract List<DieOnDropSettings?> DieOnDropSettings { get; set; }
        public abstract List<CantDropSettings?> CantDropSettings { get; set; }
        public abstract List<DisguiseSettings?> DisguiseSettings { get; set; }
        public abstract List<CraftableSettings?> CraftableSettings { get; set; }
        public abstract List<DieOnUseSettings?> DieOnUseSettings { get; set; }
        public abstract List<HealOnKillSettings?> HealOnKillSettings { get; set; }
    }
}