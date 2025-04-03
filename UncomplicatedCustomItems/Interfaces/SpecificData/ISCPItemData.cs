using Exiled.API.Enums;

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
        public abstract EffectType Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
    }

    /// <summary>
    /// The interface associated with <see cref="ItemType.SCP207"/> or <see cref="ItemType.AntiSCP207"/> <see cref="CustomItemType.SCPItem"/>
    /// </summary>
    public interface ISCP207Data
    {
        public abstract EffectType Effect { get; set; }
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
        public abstract EffectType Effect { get; set; }
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
        public abstract EffectType Effect { get; set; }
        public abstract float Duration { get; set; }
        public abstract byte Intensity { get; set; }
    }
}