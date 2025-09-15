using System.Collections.Generic;
using PlayerRoles;
using UncomplicatedCustomItems.API.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class DisguiseSettings : IDisguiseSettings
    {
        public bool RevealWhenDamaged { get; set; }
        public RoleTypeId? RoleId { get; set; } = RoleTypeId.NtfSpecialist;
        public string? DisguiseMessage { get; set; } = "Your are disguised as an NtfSpecialist!";
        public string? CustomInfo { get; set; } = string.Empty;
    }
}