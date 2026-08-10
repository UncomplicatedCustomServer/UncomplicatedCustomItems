#if EXILED
using Exiled.CustomRoles.API.Features;
#endif

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using System;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.Integrations;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class SwitchRoleOn : CustomModuleBase
    {
        public override string Name => "SwitchRoleOn";

        public float Delay { get; set; }
        public string RoleType { get; set; } = string.Empty;
        public uint RoleId { get; set; }
        public RoleSpawnFlags SpawnFlags { get; set; } = RoleSpawnFlags.All;
        public bool KeepLocation { get; set; }
        public TriggerOn Trigger { get; set; }

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
            if (CustomItem == null || player == null)
                return;

            string type = RoleType.ToLower();

            if (type == "ucr")
            {
                if (UCR.TryGetCustomRole((int)RoleId, out _))
                {
                    if (Delay > 0f)
                    {
                        Timing.CallDelayed(Delay, () => UCR.GiveCustomRole((int)RoleId, player));
                    }
                    else
                    {
                        UCR.GiveCustomRole((int)RoleId, player);
                    }

                    if (KeepLocation)
                    {
                        Vector3 oldPos = player.Position;
                        Timing.CallDelayed(0.1f, () => player.Position = oldPos);
                    }
                }
                else
                {
                    LogManager.Warn($"{RoleId} Is not a UCR role");
                }
            }
#if EXILED
            else if (type == "ecr")
            {
                if (CustomRole.TryGet(RoleId, out CustomRole? ECRRole) && ECRRole != null)
                {
                    if (Delay > 0f)
                    {
                        Timing.CallDelayed(Delay, () => ECRRole.AddRole(player));
                    }
                    else
                    {
                        ECRRole.AddRole(player);
                    }

                    if (KeepLocation)
                    {
                        Vector3 oldPos = player.Position;
                        Timing.CallDelayed(0.1f, () => player.Position = oldPos);
                    }
                }
                else
                    LogManager.Warn($"{RoleId} Is not an ECR role");
            }
#endif
            else if (type == "normal")
            {
                if (player.Role != (RoleTypeId)RoleId)
                {
                    if (Delay > 0f)
                    {
                        Timing.CallDelayed(Delay, () => player.SetRole((RoleTypeId)RoleId, RoleChangeReason.ItemUsage, SpawnFlags));
                    }
                    else
                        player.SetRole((RoleTypeId)RoleId, RoleChangeReason.ItemUsage, SpawnFlags);
                }
            }
            else
            {
                LogManager.Warn($"The role_type field in {CustomItem.Name} is currently '{RoleType}'");
            }
        }
    }
}
