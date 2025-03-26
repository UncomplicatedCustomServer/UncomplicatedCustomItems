using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class SCP500Data : ISCP500Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
    }

    public class SCP207Data : ISCP207Data
    {
        public EffectType Effect { get; set; } = new();
        public float Duration { get; set; } = 20;
        public byte Intensity { get; set; } = 1;
        public bool ApplyBaseEffect { get; set; } = false;
    }

    public class SCP018Data : ISCP018Data
    {
        public float FriendlyFireTime { get; set; } = 2f;

        public float FuseTime { get; set; } = 2f;

    }

    public class SCP268Data : ISCP268Data
    {
        
    }
    public class SCP330Data : ISCP330Data
    {
        
    }

    public class SCP2176Data : ISCP2176Data
    {
        public float FuseTime { get; set; } = 2f;

    }
    public class SCP244aData : ISCP244aData
    {
        
    }
    public class SCP244bData : ISCP244bData
    {
        
    }

    public class SCP1853Data : ISCP1853Data
    {
        
    }
    public class SCP1576Data : ISCP1576Data
    {
        
    }
    public class SCP1344Data : ISCP1344Data
    {
        
    }
}