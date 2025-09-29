namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP244 : APICustomItem
    {
        public abstract float ActivationDot { get; set; }
        public abstract float Health { get; set; }
        public abstract float MaxDiameter { get; set; }
        public abstract bool Primed { get; set; }
    }
}