#if EXILED
using Exiled.CustomRoles.API.Features;
#endif

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class SwitchRoleOn : CustomModuleBase
    {
        public override string Name => "SwitchRoleOn";
        public override List<string> RequiredArguments =>
        [
            "Delay",
            "RoleType",
            "RoleId",
            "SpawnFlags",
            "KeepLocation",
            "Trigger",
        ];

        public float Delay { get; set; }
        public string RoleType { get; set; }
        public uint RoleId { get; set; }
        public RoleSpawnFlags SpawnFlags { get; set; }
        public bool KeepLocation { get; set; }
        public TriggerOn Trigger { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<float>("Delay", out var delay))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Delay is not a valid float!");
                    return;
                }
                if (!args.TryGetValue<string>("RoleType", out var roleType))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RoleType is not a valid string!");
                    return;
                }
                if (!args.TryGetValue<uint>("RoleId", out var roleId))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RoleId is not a valid uint!");
                    return;
                }
                if (!args.TryGetValue<RoleSpawnFlags>("SpawnFlags", out var spawnFlags))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} SpawnFlags is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(RoleSpawnFlags)))}");
                    return;
                }
                if (!args.TryGetValue<bool>("KeepLocation", out var keepLocation))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} KeepLocation is not a valid bool!");
                    return;
                }
                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                Delay = delay;
                RoleType = roleType;
                RoleId = roleId;
                SpawnFlags = spawnFlags;
                KeepLocation = keepLocation;
                Trigger = trigger;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            switch (eventArgs)
            {
                case PlayerShotWeaponEventArgs shotWeaponEvent when HasFlagFast(Trigger, TriggerOn.OnShot):
                    SwitchRole(shotWeaponEvent.Player);
                    break;
                case PlayerUsedItemEventArgs usedItemEvent when HasFlagFast(Trigger, TriggerOn.OnUse):
                    SwitchRole(usedItemEvent.Player);
                    break;
                case PlayerChangedItemEventArgs changedItemEvent when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    SwitchRole(changedItemEvent.Player);
                    break;
                case PlayerPickedUpItemEventArgs pickedUpItemEvent when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    SwitchRole(pickedUpItemEvent.Player);
                    break;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ShotWeapon += Run;
            PlayerEvents.UsedItem += Run;
            PlayerEvents.ChangedItem += Run;
            PlayerEvents.PickedUpItem += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ShotWeapon -= Run;
            PlayerEvents.UsedItem -= Run;
            PlayerEvents.ChangedItem -= Run;
            PlayerEvents.PickedUpItem -= Run;
        }

        private void SwitchRole(Player player)
        {
            if (RoleType.ToLower() == "ucr")
            {
                if (UCR.TryGetCustomRole((int)RoleId, out _))
                {
                    if (Delay > 0f)
                    {
                        Timing.CallDelayed((float)Delay, () =>
                        {
                            UCR.GiveCustomRole((int)RoleId, player);
                        });
                    }
                    else
                    {
                        UCR.GiveCustomRole((int)RoleId, player);
                    }
                    if (KeepLocation)
                    {
                        Vector3 OldPos = player.Position;
                        Timing.CallDelayed(0.1f, () =>
                        {
                            player.Position = OldPos;
                        });
                    }
                }
                else
                {
                    LogManager.Warn($"{RoleId} Is not a UCR role");
                }
            }
#if EXILED
                    else if (RoleType.ToLower() == "ecr")
                    {
                        if (CustomRole.TryGet((uint)RoleId, out CustomRole? ECRRole))
                        {
                            if (Delay > 0f)
                            {
                                Timing.CallDelayed((float)Delay, () =>
                                {
                                    ECRRole.AddRole(player);
                                });
                            }
                            else
                            {
                                ECRRole.AddRole(player);
                            }

                            if (KeepLocation)
                            {
                                Vector3 OldPos = player.Position;
                                Timing.CallDelayed(0.1f, () =>
                                {
                                    player.Position = OldPos;
                                });
                            }
                        }
                        else
                            LogManager.Warn($"{RoleId} Is not a ECR role");
                    }
#endif
            else if (RoleType.ToLower() == "normal")
            {
                if (player.Role != (RoleTypeId)RoleId)
                {
                    if (Delay > 0f)
                    {
                        Timing.CallDelayed((float)Delay, () =>
                        {
                            player.SetRole((RoleTypeId)RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)SpawnFlags);
                        });
                    }
                    else
                        player.SetRole((RoleTypeId)RoleId, RoleChangeReason.ItemUsage, (RoleSpawnFlags)SpawnFlags);
                }
            }
#if EXILED
            else if (RoleType.ToLower() != "ucr" || RoleType.ToLower() != "normal" || RoleType.ToLower() != "ecr")
#else
            else if (RoleType.ToLower() != "ucr" || RoleType.ToLower() != "normal")
#endif
            {
#if EXILED
                LogManager.Warn($"The role_type field in {CustomItem.Name} is currently {RoleType} and should be 'Normal', 'UCR', or 'ECR'");
#else
                LogManager.Warn($"The role_type field in {CustomItem.Name} is currently {RoleType} and should be 'Normal' or 'UCR'");
#endif
            }
        }
    }
}
