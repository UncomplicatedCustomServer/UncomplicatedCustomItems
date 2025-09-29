namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP207 : APICustomItem
    {
        public abstract string Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool Apply207Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }
}