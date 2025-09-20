namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP268 : BaseCustomItem
    {
        public abstract bool ApplyScp268Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract bool AllowOpeningDoors { get; set; }
        public abstract bool AllowUsingElevators { get; set; }
        public abstract bool AllowOpeningLockers { get; set; }
        public abstract bool AllowOpeningGenerators { get; set; }
        public abstract bool AllowShooting { get; set; }
        public abstract bool AllowEquipingItems { get; set; }
        public abstract bool OneTimeUse { get; set; }
        public abstract float Cooldown { get; set; }
    }
}