using System.Collections.Generic;

namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface IItemGlowSettings
    {
        public string GlowColor { get; set; }
        public float Intensity { get; set; }
        public float Range { get; set; }
    }
}