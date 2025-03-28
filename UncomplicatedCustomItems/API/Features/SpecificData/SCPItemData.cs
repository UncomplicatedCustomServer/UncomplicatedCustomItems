using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class SCPItemData : Data, ISCPItemData
    {

    }
    public class SCP500Data : ISCP500Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
        public bool HealPlayer { get; set; } = false;
    }

    public class SCP207Data : ISCP207Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
        public bool Apply207Effect { get; set; } = false;
        public bool RemoveItemAfterUse { get; set; } = true;
    }

    public class SCP018Data : ISCP018Data
    {
        public float FriendlyFireTime { get; set; } = 2f;
        public float FuseTime { get; set; } = 2f;
    }

    public class SCP330Data : ISCP330Data
    {

    }

    public class SCP2176Data : ISCP2176Data
    {
        public float FuseTime { get; set; } = 2f;
    }
    public class SCP244Data : ISCP244Data
    {
        public float ActivationDot { get; set; } = 1f;
        public float Health { get; set; } = 1f;
        public float MaxDiameter { get; set; } = 1f;
        public bool Primed { get; set; } = false;

    }
    public class SCP1853Data : ISCP1853Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
        public bool Apply1853Effect { get; set; } = false;
        public bool RemoveItemAfterUse { get; set; } = true;
    }
    public class SCP1576Data : ISCP1576Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
    }
}