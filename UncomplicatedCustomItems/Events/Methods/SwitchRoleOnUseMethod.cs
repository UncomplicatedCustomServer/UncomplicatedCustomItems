#if EXILED
using Exiled.CustomRoles.API.Features;
#endif
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
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
                foreach (SwitchRoleOnUseSettings switchRoleOnUseSettings in CustomItem.CustomItem.FlagSettings.SwitchRoleOnUseSettings)
                {
                    if (switchRoleOnUseSettings.RoleId == null || switchRoleOnUseSettings.RoleType == null || switchRoleOnUseSettings == null)
                    {
                        LogManager.Warn($"{nameof(Start)}: {CustomItem.CustomItem.Name} field role_id or role_type is null aborting...");
                        break;
                    }

                    if (switchRoleOnUseSettings.RoleType.ToLower() == "ucr")
                    {
                        if (UCR.TryGetCustomRole((int)switchRoleOnUseSettings.RoleId, out _))
                        {
                            if (switchRoleOnUseSettings.Delay != null || switchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)switchRoleOnUseSettings.Delay, () =>
                                {
                                    UCR.GiveCustomRole((int)switchRoleOnUseSettings.RoleId, player);
                                });
                            }
                            else
                            {
                                UCR.GiveCustomRole((int)switchRoleOnUseSettings.RoleId, player);
                            }
                            if (switchRoleOnUseSettings.KeepLocation != null || switchRoleOnUseSettings.KeepLocation != false)
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
                            LogManager.Warn($"{nameof(Start)}: {switchRoleOnUseSettings.RoleId} Is not a UCR role");
                        }
                    }
#if EXILED
                    else if (switchRoleOnUseSettings.RoleType.ToLower() == "ecr")
                    {
                        if (CustomRole.TryGet((uint)switchRoleOnUseSettings.RoleId, out CustomRole? ECRRole))
                        {
                            if (switchRoleOnUseSettings.Delay != null || switchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)switchRoleOnUseSettings.Delay, () =>
                                {
                                    ECRRole.AddRole(player);
                                });
                            }
                            else
                            {
                                ECRRole.AddRole(player);
                            }

                            if (switchRoleOnUseSettings.KeepLocation != null || switchRoleOnUseSettings.KeepLocation != false)
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
                            LogManager.Warn($"{nameof(Start)}: {switchRoleOnUseSettings.RoleId} Is not a ECR role");
                        }
                    }
#endif
                    else if (switchRoleOnUseSettings.RoleType.ToLower() == "normal")
                    {
                        if (player.Role != (RoleTypeId)switchRoleOnUseSettings.RoleId)
                        {
                            if (switchRoleOnUseSettings.Delay != null || switchRoleOnUseSettings.Delay > 0f)
                            {
                                Timing.CallDelayed((float)switchRoleOnUseSettings.Delay, () =>
                                {
                                    player.SetRole((RoleTypeId)switchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)switchRoleOnUseSettings.SpawnFlags);
                                });
                            }
                            else
                            {
                                player.SetRole((RoleTypeId)switchRoleOnUseSettings.RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)switchRoleOnUseSettings.SpawnFlags);
                            }

                            break;
                        }
                    }
#if EXILED
                    else if (switchRoleOnUseSettings.RoleType.ToLower() != "ucr" || switchRoleOnUseSettings.RoleType.ToLower() != "normal" || switchRoleOnUseSettings.RoleType.ToLower() != "ecr")
#else
                    else if (switchRoleOnUseSettings.RoleType.ToLower() != "ucr" || switchRoleOnUseSettings.RoleType.ToLower() != "normal")
#endif
                    {
#if EXILED
                        LogManager.Warn($"{nameof(Start)}: The role_type field in {CustomItem.CustomItem.Name} is currently {switchRoleOnUseSettings.RoleType} and should be 'Normal', 'UCR', or 'ECR'");
#else
                        LogManager.Warn($"{nameof(Start)}: The role_type field in {CustomItem.CustomItem.Name} is currently {switchRoleOnUseSettings.RoleType} and should be 'Normal' or 'UCR'");
#endif      
                    }
                }
            }
        }
    }
}