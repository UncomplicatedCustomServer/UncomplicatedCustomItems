using System;

namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomSCP1853 : SCPCustomItem
    {
        [Obsolete]
        public virtual string Effect { get; set; } = string.Empty;
        [Obsolete]
        public virtual float Duration { get; set; }
        [Obsolete]
        public virtual byte Intensity { get; set; }
        public abstract bool Apply1853Effect { get; set; }
        public abstract bool RemoveItemAfterUse { get; set; }
    }
}