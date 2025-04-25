using System.Collections.Generic;
using UncomplicatedCustomItems.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class DieOnDropSettings : IDieOnDropSettings
    {
        public string? DeathMessage { get; set; } = "Killed by %name%";
        public bool? Vaporize { get; set; } = false;

    }
}