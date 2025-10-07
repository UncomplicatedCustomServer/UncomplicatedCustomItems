namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP1853 : SCPCustomItem
    {
        public abstract string Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool Apply1853Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }
}