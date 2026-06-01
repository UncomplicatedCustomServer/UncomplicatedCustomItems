namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP127 : CustomWeapon
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
        public abstract bool MuteVoiceLines { get; set; }
        public virtual bool AllowXPGain { get; set; } = true;
    }
}