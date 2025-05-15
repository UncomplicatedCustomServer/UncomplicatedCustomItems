using CustomPlayerEffects;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    #nullable enable
    /// <summary>
    /// Flag settings for <see cref="ICustomItem"/>s
    /// </summary>
    public class FlagSettings : IFlagSettings
    {
        [Description("Sets the settings for the ItemGlow flag.")]
        public virtual List<ItemGlowSettings?> ItemGlowSettings { get; set; } =
        [
            new()
            {
                GlowColor = "#00FF00"
            }
        ];
        [Description("Sets the settings for the LifeSteal flag.")]
        public virtual List<LifeStealSettings?> LifeStealSettings { get; set; } =
        [
            new()
            {
                LifeStealAmount = 8f,
                LifeStealPercentage = 0.5f,
            }
        ];
        [Description("Sets the settings for the Effect flags.")]
        public virtual List<EffectSettings?> EffectSettings { get; set; } = 
        [
            new()
            {
                EffectEvent = "EffectWhenUsed",
                Effect = typeof(Blurred),
                EffectDuration = 1,
                EffectIntensity = 1
            }
        ];
        [Description("Sets the settings for the CustomSound flag.")]
        public virtual List<AudioSettings?> AudioSettings { get; set; } = 
        [
            new()
            {
                AudioPath = "~.config/EXILED/Example.ogg",
                SoundVolume = 20f,
                AudibleDistance = 20f
            }
        ];
        [Description("Sets the settings for the ExplosiveBullets flag.")]
        public virtual List<ExplosiveBulletsSettings?> ExplosiveBulletsSettings { get; set; } = 
        [
            new()
            {
                DamageRadius = 1f
            }
        ];
        /// <summary>
        /// Sets the settings for the SpawnItemWhenDetonated flag.
        /// </summary>
        [Description("Sets the settings for the SpawnItemWhenDetonated flag.")]
        public virtual List<SpawnItemWhenDetonatedSettings?> SpawnItemWhenDetonatedSettings { get; set; } =
        [
            new()
            {
                ItemType = "Normal",
                ItemId = 1,
                Chance = 100,
                TimeTillDespawn = 6f,
                Pickupable = false
            }
        ];
        public virtual List<ClusterSettings?> ClusterSettings { get; set; } =
        [
            new()
            {
                ItemToSpawn = new(),
                AmountToSpawn = 6
            }
        ];
        public virtual List<SwitchRoleOnUseSettings?> SwitchRoleOnUseSettings { get; set; } =
        [
            new()
            {
                RoleType = "Normal",
                RoleId = new(),
                Delay = 6,
                SpawnFlags = PlayerRoles.RoleSpawnFlags.None,
                KeepLocation = true
            }
        ];
        public virtual List<DieOnDropSettings?> DieOnDropSettings { get; set; } =
        [
            new()
            {
                DeathMessage = "Dropped %name%",
                Vaporize = true
            }
        ];
        public virtual List<CantDropSettings?> CantDropSettings { get; set; } =
        [
            new()
            {
                HintOrBroadcast = "hint",
                Message = "You cant drop %name%!",
                Duration = 10
            }
        ];
        [Description("This is recommended to only be applied to armor.")]
        public virtual List<DisguiseSettings?> DisguiseSettings { get; set; } =
        [
            new()
            {
                RoleId = RoleTypeId.NtfSpecialist,
                DisguiseMessage = "Your are disguised as an NtfSpecialist!",
            }
        ];
        public virtual List<CraftableSettings?> CraftableSettings { get; set; } =
        [
            new()
            {
                KnobSetting = Scp914.Scp914KnobSetting.Coarse,
                OriginalItem = ItemType.Adrenaline,
                Chance = 100,
            }
        ];
        public virtual List<DieOnUseSettings?> DieOnUseSettings { get; set; } =
        [
            new()
            {
                DeathMessage = "Killed by %name%",
                Vaporize = false,
            }
        ];
        public virtual List<HealOnKillSettings?> HealOnKillSettings { get; set; } =
        [
            new()
            {
                HealAmount = 1f,
                ConvertToAhpIfFull = false,
            }
        ];
    }
}
