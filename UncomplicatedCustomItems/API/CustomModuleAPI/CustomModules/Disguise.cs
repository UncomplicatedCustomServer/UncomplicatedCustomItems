using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using PlayerRoles;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Integrations;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Disguise : CustomModuleBase
    {
        public static Dictionary<int, RoleTypeId> Appearance = [];
        public override string Name => "Disguise";
        public override List<string> RequiredArguments =>
        [
            "RevealWhenDamaged",
            "RoleId",
            "DisguiseMessage",
            "CustomInfo",
            "Trigger"
        ];

        public bool RevealWhenDamaged { get; set; }
        public RoleTypeId RoleId { get; set; }
        public string DisguiseMessage { get; set; }
        public string CustomInfo { get; set; }
        public TriggerOn Trigger { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<bool>("RevealWhenDamaged", out var revealWhenDamaged))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RevealWhenDamaged is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<string>("DisguiseMessage", out var disguiseMessage))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} DisguiseMessage is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<string>("CustomInfo", out var customInfo))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} CustomInfo is not a valid string!");
                    return;
                }


                if (!args.TryGetValue<RoleTypeId>("RoleId", out var roleId))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RoleId is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(RoleTypeId)))}!");
                    return;
                }

                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                RevealWhenDamaged = revealWhenDamaged;
                DisguiseMessage = disguiseMessage;
                CustomInfo = customInfo;
                RoleId = roleId;
                Trigger = trigger;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            base.Run(eventArgs);
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
                    LogManager.Debug($"Adding or updating {playerused.Id} to appearance dictionary");
                    Appearance.TryAdd(playerused.Id, RoleId);
#else
                    LogManager.Debug($"Changing {playerUsed.Player.Nickname} appearance to {RoleId}");
                    playerUsed.Player.DisguisePlayer(RoleId);
                    playerUsed.Player.SendBroadcast($"{DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                    playerUsed.Player.CustomInfo = CustomInfo;
                    LogManager.Debug($"Adding or updating {playerUsed.Player.PlayerId} to appearance dictionary");
                    Appearance.TryAdd(playerUsed.Player.PlayerId, RoleId);
#endif
                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
#if EXILED
                    Exiled.API.Features.Player playerpickedup = Exiled.API.Features.Player.Get(playerPickedUpItem.Player);
                    LogManager.Debug($"Changing {playerpickedup.DisplayNickname} appearance to {RoleId}");
                    Exiled.API.Extensions.MirrorExtensions.ChangeAppearance(playerpickedup, RoleId);
                    playerpickedup.Broadcast(10, $"{DisguiseMessage}", Broadcast.BroadcastFlags.Normal, true);
                    playerPickedUpItem.Player.CustomInfo = CustomInfo;
                    LogManager.Debug($"Adding or updating {playerpickedup.Id} to appearance dictionary");
                    Appearance.TryAdd(playerpickedup.Id, RoleId);
#else
                    LogManager.Debug($"Changing {playerPickedUpItem.Player.Nickname} appearance to {RoleId}");
                    playerPickedUpItem.Player.DisguisePlayer(RoleId);
                    playerPickedUpItem.Player.SendBroadcast($"{DisguiseMessage}", 10, Broadcast.BroadcastFlags.Normal, true);
                    playerPickedUpItem.Player.CustomInfo = CustomInfo;
                    LogManager.Debug($"Adding or updating {playerPickedUpItem.Player.PlayerId} to appearance dictionary");
                    Appearance.TryAdd(playerPickedUpItem.Player.PlayerId, RoleId);
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
                    LogManager.Debug($"Adding or updating {explayer.Id} to appearance dictionary");
                    Appearance.TryAdd(explayer.Id, ev.Player.Role);
#else
                    LogManager.Debug($"Changing {ev.Player.Nickname} appearance to {ev.Player.Role}");
                    ev.Attacker.DisguisePlayer(ev.Player.Role);
                    LogManager.Debug($"Adding or updating {ev.Player.PlayerId} to appearance dictionary");
                    Appearance.TryAdd(ev.Player.PlayerId, ev.Player.Role);
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