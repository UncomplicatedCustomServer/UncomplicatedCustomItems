using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    #nullable enable
    /// <summary>
    /// Flag settings for <see cref="ICustomItem"/>
    /// </summary>
    public class FlagSettings : IFlagSettings
    {
        /*
        /// <summary>
        /// Sets the glow color of the <see cref="ICustomItem"/> if it has the ItemGlow <see cref="ICustomModule"/>.
        /// </summary>
        [Description("Sets the glow color of the item if it has the ItemGlow custom flag.")]
        public string GlowColor { get; set; } = "#00FF00";
        */
        [Description("Sets the settings for the ItemGlow flag.")]
        public virtual List<ItemGlowSettings?> ItemGlowSettings { get; set; } =
        [
            new()
            {
                GlowColor = "#00FF00"
            }
        ];
        /*
        /// <summary>
        /// Sets the life steal amount of the <see cref="ICustomItem"/> if it has the LifeSteal <see cref="ICustomModule"/>.
        /// </summary>
        [Description("Sets the life steal amount of the item if it has the LifeSteal custom flag.")]
        public float LifeStealAmount { get; set; } = 8f;

        /// <summary>
        /// Sets the percentage of health regenerated if the <see cref="ICustomItem"/> has the HalfLifeSteal <see cref="ICustomModule"/>.
        /// </summary>
        [Description("Sets the percentage of health regenerated if the item has the HalfLifeSteal custom flag. HealedAmount = Amount * Percentage")]
        public float LifeStealPercentage { get; set; } = 0.5f;
        */
        [Description("Sets the settings for the LifeSteal flag.")]
        public virtual List<LifeStealSettings?> LifeStealSettings { get; set; } =
        [
            new()
            {
                LifeStealAmount = 8f,
                LifeStealPercentage = 0.5f,
            }
        ];
        /*
        /// <summary>
        /// Sets the effect event that should be triggered by the <see cref="ICustomItem"/>. 
        /// This should be modified based on the associated <see cref="ICustomModule"/>.
        /// </summary>
        [Description("Sets the effect event that should be triggered by the custom item. Modify this based on the associated flag.")]
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
        */
        [Description("Sets the settings for the Effect flags.")]
        public virtual List<EffectSettings?> EffectSettings { get; set; } = 
        [
            new()
            {
                EffectEvent = "EffectWhenUsed",
                Effect = EffectType.AntiScp207,
                EffectDuration = 1,
                EffectIntensity = 1
            }
        ];
        /*
        /// <summary>
        /// Tells the <see cref="AudioApi"/> where the ogg audio file is.
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
        */
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
        /*
        /// <summary>
        /// Sets the damage radius of the ExplosiveBullets flag.
        /// </summary>
        [Description("Sets the damage radius of the ExplosiveBullets flag.")]
        public float? DamageRadius { get; set; } = 1f;
        */
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
