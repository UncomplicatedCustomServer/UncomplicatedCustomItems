namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IFlashGrenadeData : IData
    {
        public abstract float MinimalDurationEffect { get; set; }

        public abstract float AdditionalBlindedEffect { get; set; }

        public abstract float SurfaceDistanceIntensifier { get; set; }

        public abstract float FuseTime {  get; set; }

        public abstract float PinPullTime { get; set; }

        public abstract bool Repickable { get; set; }
    }
}
