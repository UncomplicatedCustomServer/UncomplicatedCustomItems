using Exiled.API.Enums;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface ISCPItemData : IData
    {

    }
    public interface ISCP500Data
    {
        public abstract EffectType Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool HealPlayer { get; set; }
    }

    public interface ISCP207Data
    {
        public abstract EffectType Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool Apply207Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }
    public interface ISCP018Data : IData
    {
        public abstract float FriendlyFireTime { get; set; }

        public abstract float FuseTime { get; set; }
    }

    public interface ISCP330Data
    {

    }

    public interface ISCP2176Data : IData
    {
        public abstract float FuseTime { get; set; }
    }
    public interface ISCP244Data : IData
    {
        public abstract float ActivationDot { get; set; }
        public abstract float Health { get; set; }
        public abstract float MaxDiameter { get; set; }
        public abstract bool Primed { get; set; }
    }
    public interface ISCP1853Data
    {
        public abstract EffectType Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
        public abstract bool Apply1853Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }
    public interface ISCP1576Data
    {
        public abstract EffectType Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
    }
}