namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP1509 : APICustomItem
    {
        public abstract bool CanResurrect { get; set; }
        public abstract double ReviveCooldown { get; set; }
        public abstract float RevivedPlayerMaxahp { get; set; }
        public abstract float RevivedPlayeraoeBonusahp { get; set; }
        public abstract float HumeshieldMax { get; set; }
        public abstract float HumeshieldRegeneration { get; set; }
        public abstract float HumeshieldRegenRate { get; set; }
        public abstract float HumeshieldDecayRate { get; set; }
        public abstract float HumeshieldOnDamagePauseTime { get; set; }
        public abstract float UnequipHumeshieldDecayDelay { get; set; }
    }
}