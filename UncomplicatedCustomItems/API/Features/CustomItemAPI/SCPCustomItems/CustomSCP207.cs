using System;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP207 : SCPCustomItem
    {
        [Obsolete]
        public abstract string Effect { get; set; }
        [Obsolete]
        public abstract float Duration { get; set; }
        [Obsolete]
        public abstract byte Intensity { get; set; }

        public float InstantHealth { get; set; }
        public bool BypassMax { get; set; }
        public float StaminaGain { get; set; }
        public abstract bool Apply207Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }
}