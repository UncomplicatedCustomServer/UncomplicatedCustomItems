using PlayerRoles;

namespace UncomplicatedCustomItems.Interfaces.FlagSettings
{
    internal interface IDisguiseSettings
    {
        public abstract RoleTypeId? RoleId { get; set; }
        public abstract string? DisguiseMessage { get; set; }
    }
}
