using System.Collections.Generic;
using PlayerRoles;

namespace UncomplicatedCustomItems.API.Interfaces.FlagSettings
{
    public interface IDisguiseSettings
    {
        public bool RevealWhenDamaged { get; set; }
        public abstract RoleTypeId? RoleId { get; set; }
        public abstract string? DisguiseMessage { get; set; }
        public string? CustomInfo { get; set; }
    }
}