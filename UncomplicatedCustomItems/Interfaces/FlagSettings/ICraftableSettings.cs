using Scp914;

namespace UncomplicatedCustomItems.Interfaces.FlagSettings
{
    internal interface ICraftableSettings
    {
        public abstract Scp914KnobSetting? KnobSetting { get; set; }
        public abstract ItemType? OriginalItem { get; set; }
        public abstract int? Chance { get; set; }
    }
}
