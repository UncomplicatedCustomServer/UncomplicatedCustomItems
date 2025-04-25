using System.Collections.Generic;
using PlayerRoles;

namespace UncomplicatedCustomItems.Interfaces
{
    public interface ISwitchRoleOnUseSettings
    {
        public abstract float? Delay { get; set; }
        public abstract string? RoleType { get; set; }
        public abstract uint? RoleId { get; set; }
        public abstract RoleSpawnFlags? SpawnFlags { get; set; }
        public abstract bool? KeepLocation { get; set; }
    }
}