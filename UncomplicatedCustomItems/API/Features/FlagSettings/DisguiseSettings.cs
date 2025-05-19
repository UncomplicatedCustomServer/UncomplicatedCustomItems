using PlayerRoles;
using UncomplicatedCustomItems.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class DisguiseSettings : IDisguiseSettings
    {
        public RoleTypeId? RoleId { get; set; } = RoleTypeId.NtfSpecialist;
        public string? DisguiseMessage { get; set; } = "Your are disguised as an NtfSpecialist!";
    }
}