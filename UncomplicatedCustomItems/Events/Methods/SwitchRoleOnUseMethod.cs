using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Enums;
using UnityEngine;

namespace UncomplicatedCustomItems.Events.Methods
{
    // Testing this to hopefully simplify the EventHandler code.
    public class SwitchRoleOnUseMethod
    {
        public static void Start(SummonedCustomItem CustomItem, Player player)
        {
            if (CustomItem.CustomItem.CustomFlags.HasValue && CustomItem.CustomItem.CustomFlags.Value.HasFlag(CustomFlags.SwitchRoleOnUse))
            {
                foreach (SwitchRoleOnUseSettings SwitchRoleOnUseSettings in CustomItem.CustomItem.FlagSettings.SwitchRoleOnUseSettings)
                {
                    if (SwitchRoleOnUseSettings.RoleId == null || SwitchRoleOnUseSettings.RoleType == null)
                    {
                        LogManager.Warn($"{nameof(Start)}: {CustomItem.CustomItem.Name} field role_id or role_type is null aborting...");
                        break;
                    }
                    else if (SwitchRoleOnUseSettings.RoleType == "Normal")
                    {
                        if (player.Role != (RoleTypeId)SwitchRoleOnUseSettings.RoleId)
                        {
                            if (SwitchRoleOnUseSettings.Delay != null || SwitchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                {
                                    player.SetRole((RoleTypeId)SwitchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)SwitchRoleOnUseSettings.SpawnFlags);
                                });
                            }
                            else
                            {
                                player.SetRole((RoleTypeId)SwitchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)SwitchRoleOnUseSettings.SpawnFlags);
                            }
                            break;
                        }
                    }
                    else if (SwitchRoleOnUseSettings.RoleType != "ECR" || SwitchRoleOnUseSettings.RoleType != "UCR" || SwitchRoleOnUseSettings.RoleType != "Normal")
                    {
                        LogManager.Warn($"{nameof(Start)}: The role_type field in {CustomItem.CustomItem.Name} is currently {SwitchRoleOnUseSettings.RoleType} and should be 'Normal', 'UCR', or 'ECR'");
                    }
                }
            }
        }
    }
}