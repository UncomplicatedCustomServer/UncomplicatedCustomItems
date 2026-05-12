using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class CustomAudio : CustomModuleBase
    {
        public override string Name => "CustomAudio";
        public override List<string> RequiredArguments => 
        [
            "Trigger",
            "AudioPath",
            "AudibleDistance",
            "SoundVolume",
        ];

        public TriggerOn Trigger { get; set; }
        public string AudioPath { get; set; } = string.Empty;
        public float AudibleDistance { get; set; }
        public float Volume { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                if (!args.TryGetValue<string>("AudioPath", out var audioPath))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} AudioPath is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<float>("AudibleDistance", out var audibleDistance))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} AudibleDistance is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("SoundVolume", out var volume))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} SoundVolume is not a valid float!");
                    return;
                }

                Trigger = trigger;
                AudioPath = audioPath;
                AudibleDistance = audibleDistance;
                Volume = volume;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            switch (eventArgs)
            {
                case PlayerInteractedDoorEventArgs playerInteractedDoor when HasFlagFast(Trigger, TriggerOn.OnDoorInteracted):
                    AudioApi.PlayAudio(AudioPath, Volume, playerInteractedDoor.Player.Position, AudibleDistance);
                    break;

                case PlayerShotWeaponEventArgs playerShotWeapon when HasFlagFast(Trigger, TriggerOn.OnShot):
                    AudioApi.PlayAudio(AudioPath, Volume, playerShotWeapon.Player.Position, AudibleDistance);
                    break;

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnUse):
                    AudioApi.PlayAudio(AudioPath, Volume, playerUsedItem.Player.Position, AudibleDistance);
                    break;

                case PlayerReloadedWeaponEventArgs playerReloadedWeapon when HasFlagFast(Trigger, TriggerOn.OnReload):
                    AudioApi.PlayAudio(AudioPath, Volume, playerReloadedWeapon.Player.Position, AudibleDistance);
                    break;

                case PlayerChangedItemEventArgs playerChangedItem when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    AudioApi.PlayAudio(AudioPath, Volume, playerChangedItem.Player.Position, AudibleDistance);
                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    AudioApi.PlayAudio(AudioPath, Volume, playerPickedUpItem.Player.Position, AudibleDistance);
                    break;

                case PlayerDroppedItemEventArgs playerDroppedItem when HasFlagFast(Trigger, TriggerOn.OnDropped):
                    AudioApi.PlayAudio(AudioPath, Volume, playerDroppedItem.Player.Position, AudibleDistance);
                    break;

                case PlayerDeathEventArgs playerDeath when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    AudioApi.PlayAudio(AudioPath, Volume, playerDeath.OldPosition, AudibleDistance);
                    break;
                
                case PlayerHurtEventArgs playerHurt when HasFlagFast(Trigger, TriggerOn.OnHurt):
                    AudioApi.PlayAudio(AudioPath, Volume, playerHurt.Player.Position, AudibleDistance);
                    break;
            }
        }

        public override void RegisterEvents()
        {
            base.RegisterEvents();

            PlayerEvents.InteractedDoor += Run;
            PlayerEvents.ShotWeapon += Run;
            PlayerEvents.UsedItem += Run;
            PlayerEvents.ReloadedWeapon += Run;
            PlayerEvents.ChangedItem += Run;
            PlayerEvents.PickedUpItem += Run;
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.Death += Run;
            PlayerEvents.Hurt += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.InteractedDoor -= Run;
            PlayerEvents.ShotWeapon -= Run;
            PlayerEvents.UsedItem -= Run;
            PlayerEvents.ReloadedWeapon -= Run;
            PlayerEvents.ChangedItem -= Run;
            PlayerEvents.PickedUpItem -= Run;
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.Death -= Run;
            PlayerEvents.Hurt -= Run;
        }
    }
}