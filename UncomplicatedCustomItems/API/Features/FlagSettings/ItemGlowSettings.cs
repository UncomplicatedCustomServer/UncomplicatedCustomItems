using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class ItemGlowSettings : IItemGlowSettings
    {
        public string GlowColor { get; set; } = "#00FF00";
    }
}