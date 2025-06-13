using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// The data associated with <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCPItemData : Data, ISCPItemData
    {

    }
    
    /// <summary>
    /// The data associated with <see cref="ItemType.SCP500"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP500Data : Data, ISCP500Data
    {
        public virtual EffectType Effect { get; set; } = new();
        public virtual float Duration { get; set; } = 20;
        public virtual byte Intensity { get; set; } = 1;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP207"/> or <see cref="ItemType.AntiSCP207"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP207Data : Data, ISCP207Data
    {
        public virtual EffectType Effect { get; set; } = new();
        public virtual float Duration { get; set; } = 20;
        public virtual byte Intensity { get; set; } = 1;
        public virtual bool Apply207Effect { get; set; } = false;
        public virtual bool RemoveItemAfterUse { get; set; } = true;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP018"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP018Data : Data, ISCP018Data
    {
        public virtual float FriendlyFireTime { get; set; } = 2f;
        public virtual float FuseTime { get; set; } = 2f;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP330"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// Currently unused
    /// </summary>
    public class SCP330Data : Data, ISCP330Data // Dont really know what to do for this
    {

    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP2176"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP2176Data : Data, ISCP2176Data
    {
        public virtual float FuseTime { get; set; } = 2f;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP244a"/> or <see cref="ItemType.SCP244b"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP244Data : Data, ISCP244Data
    {
        public virtual float ActivationDot { get; set; } = 1f;
        public virtual float Health { get; set; } = 1f;
        public virtual float MaxDiameter { get; set; } = 1f;
        public virtual bool Primed { get; set; } = false;

    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP1853"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP1853Data : Data, ISCP1853Data
    {
        public virtual EffectType Effect { get; set; } = new();
        public virtual float Duration { get; set; } = 20;
        public virtual byte Intensity { get; set; } = 1;
        public virtual bool Apply1853Effect { get; set; } = false;
        public virtual bool RemoveItemAfterUse { get; set; } = true;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP1576"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP1576Data : Data, ISCP1576Data
    {
        public virtual EffectType Effect { get; set; } = new();
        public virtual float Duration { get; set; } = 20;
        public virtual byte Intensity { get; set; } = 1;
    }
    public class SCP127Data : Data, ISCP127Data
    {
        public virtual bool GiveHumeShield { get; set; } = false;
        public virtual float Tier1BulletFireRate { get; set; } = 1f;
        public virtual float Tier2BulletFireRate { get; set; } = 1f;
        public virtual float Tier3BulletFireRate { get; set; } = 1f;
        public virtual float Tier1BulletRegenRate { get; set; } = 1f;
        public virtual float Tier2BulletRegenRate { get; set; } = 1f;
        public virtual float Tier3BulletRegenRate { get; set; } = 1f;
        public virtual float Tier1BulletRegenPostFireDelay { get; set; } = 1f;
        public virtual float Tier2BulletRegenPostFireDelay { get; set; } = 1f;
        public virtual float Tier3BulletRegenPostFireDelay { get; set; } = 1f;
        public virtual float Tier1HumeShieldAmount { get; set; } = 1f;
        public virtual float Tier2HumeShieldAmount { get; set; } = 1f;
        public virtual float Tier3HumeShieldAmount { get; set; } = 1f;
        public virtual float Tier1ShieldRegenRate { get; set; } = 1f;
        public virtual float Tier2ShieldRegenRate { get; set; } = 1f;
        public virtual float Tier3ShieldRegenRate { get; set; } = 1f;
        public virtual float Tier1ShieldDecayRate { get; set; } = 1f;
        public virtual float Tier2ShieldDecayRate { get; set; } = 1f;
        public virtual float Tier3ShieldDecayRate { get; set; } = 1f;
        public virtual float Tier1ShieldOnDamagePause { get; set; } = 1f;
        public virtual float Tier2ShieldOnDamagePause { get; set; } = 1f;
        public virtual float Tier3ShieldOnDamagePause { get; set; } = 1f;
        public virtual float Damage { get; set; } = 2.75f;
        public virtual int MaxBarrelAmmo { get; set; } = 10;
        public virtual int MaxAmmo { get; set; } = 150;
        public virtual int MaxMagazineAmmo { get; set; } = 150;
        public virtual int AmmoDrain { get; set; } = 1;
        public virtual float Penetration { get; set; } = 1.24f;
        public virtual float Inaccuracy { get; set; } = 1.24f;
        public virtual float AimingInaccuracy { get; set; } = 1.24f;
        public virtual float DamageFalloffDistance { get; set; } = 1f;
        public virtual bool MuteVoiceLines { get; set; } = false;
    }
}