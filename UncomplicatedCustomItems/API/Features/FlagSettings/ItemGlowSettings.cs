using System.ComponentModel;
using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class ItemGlowSettings : IItemGlowSettings
    {
        public string GlowColor { get; set; } = "#00FF00";
    }
}