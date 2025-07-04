using System.ComponentModel;
using PlayerRoles;
using UncomplicatedCustomItems.API.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class SwitchRoleOnUseSettings : ISwitchRoleOnUseSettings
    {
        public float? Delay { get; set; } = 1;

        [Description("Set this to UCR, ECR or Normal.")]
        public string? RoleType { get; set; } = "";

        [Description("Set this to the RoleTyped if Normal or customrole id for UCR or ECR.")]
        public uint? RoleId { get; set; } = 1;

        [Description("This can be set to None, AssignInventory, UseSpawnpoint, or All. Use this only if the role_type is Normal")]
        public RoleSpawnFlags? SpawnFlags { get; set; } = RoleSpawnFlags.None;

        [Description("Change this if your using ECR or UCR")]
        public bool? KeepLocation { get; set; } = false;
    }
}