using UncomplicatedCustomItems.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class DieOnUseSettings : IDieOnUseSettings
    {
        public string? DeathMessage { get; set; } = "Killed by %name%";
        public bool? Vaporize { get; set; } = false;
    }
}
