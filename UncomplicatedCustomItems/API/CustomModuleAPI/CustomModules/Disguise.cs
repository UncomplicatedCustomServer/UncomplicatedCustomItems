using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using PlayerRoles;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.Integrations;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Disguise : CustomModuleBase
    {
        public static Dictionary<int, RoleTypeId> Appearance = [];
        public override string Name => "Disguise";

        public bool RevealWhenDamaged { get; set; }
        public RoleTypeId RoleId { get; set; }
        public string DisguiseMessage { get; set; } = string.Empty;
        public string CustomInfo { get; set; } = string.Empty;
        public TriggerOn Trigger { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            switch (eventArgs)
            {
                case PlayerHurtEventArgs playerHurt when RevealWhenDamaged:
#if EXILED
                    Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(playerHurt.Player);
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(player, player.Role);
#else
                    playerHurt.Player.DisguisePlayer(playerHurt.Player.Role);
#endif
                    Appearance.Remove(playerHurt.Player.PlayerId);
                    break;

                case PlayerUsedItemEventArgs playerUsed when HasFlagFast(Trigger, TriggerOn.OnUse):
#if EXILED
                    Exiled.API.Features.Player playerused = Exiled.API.Features.Player.Get(playerUsed.Player);
                    LogManager.Debug($"Changing {playerused.DisplayNickname} appearance to {RoleId}");
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(playerused, RoleId);
                    playerused.Broadcast(10, $"{DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                    playerUsed.Player.CustomInfo = CustomInfo;
                    Appearance[playerused.Id] = RoleId;
#else
                    LogManager.Debug($"Changing {playerUsed.Player.Nickname} appearance to {RoleId}");
                    playerUsed.Player.DisguisePlayer(RoleId);
                    playerUsed.Player.SendBroadcast($"{DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                    playerUsed.Player.CustomInfo = CustomInfo;
                    Appearance[playerUsed.Player.PlayerId] = RoleId;
#endif
                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
#if EXILED
                    Exiled.API.Features.Player playerpickedup = Exiled.API.Features.Player.Get(playerPickedUpItem.Player);
                    LogManager.Debug($"Changing {playerpickedup.DisplayNickname} appearance to {RoleId}");
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(playerpickedup, RoleId);
                    playerpickedup.Broadcast(10, $"{DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                    playerPickedUpItem.Player.CustomInfo = CustomInfo;
                    Appearance[playerpickedup.Id] = RoleId;
#else
                    LogManager.Debug($"Changing {playerPickedUpItem.Player.Nickname} appearance to {RoleId}");
                    playerPickedUpItem.Player.DisguisePlayer(RoleId);
                    playerPickedUpItem.Player.SendBroadcast($"{DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                    playerPickedUpItem.Player.CustomInfo = CustomInfo;
                    Appearance[playerPickedUpItem.Player.PlayerId] = RoleId;
#endif
                    break;

                case PlayerDroppedItemEventArgs playerDroppedItem:
#if EXILED
                    Exiled.API.Features.Player playerdropped = Exiled.API.Features.Player.Get(playerDroppedItem.Player);
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(playerdropped, playerdropped.Role);
#else
                    playerDroppedItem.Player.DisguisePlayer(playerDroppedItem.Player.Role);
#endif
                    Appearance.Remove(playerDroppedItem.Player.PlayerId);
                    break;

                case PlayerDyingEventArgs ev when HasFlagFast(Trigger, TriggerOn.OnDeath):
#if EXILED
                    Exiled.API.Features.Player explayer = Exiled.API.Features.Player.Get(ev.Attacker);
                    LogManager.Debug($"Changing {explayer.DisplayNickname} appearance to {ev.Player.Role}");
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(explayer, ev.Player.Role);
                    Appearance[explayer.Id] = ev.Player.Role;
#else
                    LogManager.Debug($"Changing {ev.Player.Nickname} appearance to {ev.Player.Role}");
                    ev.Attacker?.DisguisePlayer(ev.Player.Role);
                    if (ev.Attacker != null)
                        Appearance[ev.Attacker.PlayerId] = ev.Player.Role;
#endif
                    break;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.Hurt += Run;
            PlayerEvents.UsedItem += Run;
            PlayerEvents.PickedUpItem += Run;
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.Dying += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.Hurt -= Run;
            PlayerEvents.UsedItem -= Run;
            PlayerEvents.PickedUpItem -= Run;
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.Dying -= Run;
        }
    }
}