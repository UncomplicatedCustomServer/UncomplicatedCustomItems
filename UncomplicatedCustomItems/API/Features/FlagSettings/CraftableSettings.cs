using Scp914;
using UncomplicatedCustomItems.API.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class CraftableSettings : ICraftableSettings
    {
        public Scp914KnobSetting? KnobSetting { get; set; } = Scp914KnobSetting.Coarse;
        public ItemType? OriginalItem { get; set; } = ItemType.Adrenaline;
        public int? Chance { get; set; } = 100;
    }
}