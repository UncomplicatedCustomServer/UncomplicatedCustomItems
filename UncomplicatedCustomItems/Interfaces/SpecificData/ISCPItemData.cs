using System;
using CustomPlayerEffects;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCPItemData : IData
    {

    }
    
    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP500"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP500Data
    {
        public abstract Type Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP207"/> or <see cref="ItemType.AntiSCP207"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP207Data
    {
        public abstract Type Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool Apply207Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP018"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP018Data : IData
    {
        public abstract float FriendlyFireTime { get; set; }

        public abstract float FuseTime { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP330"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP330Data
    {

    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP2176"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP2176Data : IData
    {
        public abstract float FuseTime { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP244a"/> or <see cref="ItemType.SCP244b"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP244Data : IData
    {
        public abstract float ActivationDot { get; set; }
        public abstract float Health { get; set; }
        public abstract float MaxDiameter { get; set; }
        public abstract bool Primed { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP1853"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP1853Data
    {
        public abstract Type Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool Apply1853Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP1576"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP1576Data
    {
        public abstract Type Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
    }
    public interface ISCP127Data
    {
        public abstract bool GiveHumeShield { get; set; }
        public abstract float Tier1BulletFireRate { get; set; }
        public abstract float Tier2BulletFireRate { get; set; }
        public abstract float Tier3BulletFireRate { get; set; }
        public abstract float Tier1BulletRegenRate { get; set; }
        public abstract float Tier2BulletRegenRate { get; set; }
        public abstract float Tier3BulletRegenRate { get; set; }
        public abstract float Tier1BulletRegenPostFireDelay { get; set; }
        public abstract float Tier2BulletRegenPostFireDelay { get; set; }
        public abstract float Tier3BulletRegenPostFireDelay { get; set; }
        public abstract float Tier1HumeShieldAmount { get; set; }
        public abstract float Tier2HumeShieldAmount { get; set; }
        public abstract float Tier3HumeShieldAmount { get; set; }
        public abstract float Tier1ShieldRegenRate { get; set; }
        public abstract float Tier2ShieldRegenRate { get; set; }
        public abstract float Tier3ShieldRegenRate { get; set; }
        public abstract float Tier1ShieldDecayRate { get; set; }
        public abstract float Tier2ShieldDecayRate { get; set; }
        public abstract float Tier3ShieldDecayRate { get; set; }
        public abstract float Tier1ShieldOnDamagePause { get; set; }
        public abstract float Tier2ShieldOnDamagePause { get; set; }
        public abstract float Tier3ShieldOnDamagePause { get; set; }
        public abstract float Damage { get; set; }
        public abstract int MaxBarrelAmmo { get; set; }
        public abstract int MaxAmmo { get; set; }
        public abstract int MaxMagazineAmmo { get; set; }
        public abstract int AmmoDrain { get; set; }
        public abstract float Penetration { get; set; }
        public abstract float Inaccuracy { get; set; }
        public abstract float AimingInaccuracy { get; set; }
        public abstract float DamageFalloffDistance { get; set; }
        public abstract bool MuteVoiceLines { get; set; }
    }
}