using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

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
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP207"/> or <see cref="ItemType.AntiSCP207"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP207Data : Data, ISCP207Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
        public bool Apply207Effect { get; set; } = false;
        public bool RemoveItemAfterUse { get; set; } = true;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP018"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP018Data : Data, ISCP018Data
    {
        public float FriendlyFireTime { get; set; } = 2f;
        public float FuseTime { get; set; } = 2f;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP330"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// Currently unused
    /// </summary>
    public class SCP330Data : Data, ISCP330Data //Dont really know what to do for this
    {

    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP2176"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP2176Data : Data, ISCP2176Data
    {
        public float FuseTime { get; set; } = 2f;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP244a"/> or <see cref="ItemType.SCP244b"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP244Data : Data, ISCP244Data
    {
        public float ActivationDot { get; set; } = 1f;
        public float Health { get; set; } = 1f;
        public float MaxDiameter { get; set; } = 1f;
        public bool Primed { get; set; } = false;

    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP1853"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP1853Data : Data, ISCP1853Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
        public bool Apply1853Effect { get; set; } = false;
        public bool RemoveItemAfterUse { get; set; } = true;
    }

    /// <summary>
    /// The data associated with <see cref="ItemType.SCP1576"/> <see cref="CustomItemType.SCPItem"/> <see cref="CustomItem"/>s
    /// </summary>
    public class SCP1576Data : Data, ISCP1576Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
    }
}