using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class ItemGlowSettings : IItemGlowSettings
    {
        public string GlowColor { get; set; } = "#00FF00";
        public float Intensity { get; set; } = 0.7f;
        public float Range { get; set; } = 0.5f;
    }
}