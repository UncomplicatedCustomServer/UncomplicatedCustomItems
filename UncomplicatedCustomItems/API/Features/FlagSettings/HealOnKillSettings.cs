using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class HealOnKillSettings : IHealOnKillSettings
    {
        public float? HealAmount { get; set; } = 1f;
        public bool? ConvertToAhpIfFull { get; set; } = false;
    }
}