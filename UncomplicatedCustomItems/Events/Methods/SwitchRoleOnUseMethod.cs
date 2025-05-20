using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Enums;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;

namespace UncomplicatedCustomItems.Events.Methods
{
    // Testing this to hopefully simplify the EventHandler code.
    public class SwitchRoleOnUseMethod
    {
        public static void Start(SummonedCustomItem CustomItem, Player player)
        {
            if (CustomItem.HasModule(CustomFlags.SwitchRoleOnUse))
            {
                foreach (SwitchRoleOnUseSettings SwitchRoleOnUseSettings in CustomItem.CustomItem.FlagSettings.SwitchRoleOnUseSettings)
                {
                    if (SwitchRoleOnUseSettings.RoleId == null || SwitchRoleOnUseSettings.RoleType == null || SwitchRoleOnUseSettings == null)
                    {
                        LogManager.Warn($"{nameof(Start)}: {CustomItem.CustomItem.Name} field role_id or role_type is null aborting...");
                        break;
                    }
                    if (SwitchRoleOnUseSettings.RoleType == "UCR")
                    {
                        if (UCR.TryGetCustomRole((int)SwitchRoleOnUseSettings.RoleId, out _))
                        {
                            if (SwitchRoleOnUseSettings.Delay != null || SwitchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)SwitchRoleOnUseSettings.Delay, () =>
                                {
                                    UCR.GiveCustomRole((int)SwitchRoleOnUseSettings.RoleId, player);
                                });
                            }
                            else
                            {
                                UCR.GiveCustomRole((int)SwitchRoleOnUseSettings.RoleId, player);
                            }
                            if (SwitchRoleOnUseSettings.KeepLocation != null || SwitchRoleOnUseSettings.KeepLocation != false)
                            {
                                Vector3 OldPos = player.Position;
                                Timing.CallDelayed(0.1f, () =>
                                {
                                    player.Position = OldPos;
                                });
                            }
                            break;
                        }
                        else
                        {
                            LogManager.Warn($"{nameof(Start)}: {SwitchRoleOnUseSettings.RoleId} Is not a UCR role");
                        }
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
                    else if (SwitchRoleOnUseSettings.RoleType != "UCR" || SwitchRoleOnUseSettings.RoleType != "Normal")
                    {
                        LogManager.Warn($"{nameof(Start)}: The role_type field in {CustomItem.CustomItem.Name} is currently {SwitchRoleOnUseSettings.RoleType} and should be 'Normal', 'UCR', or 'ECR'");
                    }
                }
            }
        }
    }
}