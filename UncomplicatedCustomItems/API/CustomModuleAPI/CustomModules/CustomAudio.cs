using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Integrations;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class CustomAudio : CustomModuleBase
    {
        public override string Name => "CustomAudio";
        public override List<string> RequiredArguments => 
        [
            "Trigger",
            "AudioPath",
            "MaxAudibleDistance",
            "MinAudibleDistance",
            "SoundVolume",
        ];

        public SummonedCustomItem? SummonedCustomItem { get; private set; }

        public TriggerOn Trigger { get; set; }
        public string AudioPath { get; set; } = string.Empty;
        public float MaxAudibleDistance { get; set; }
        public float MinAudibleDistance { get; set; }
        public float Volume { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            SummonedCustomItem = item;
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! Valid values: {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                if (!args.TryGetValue<string>("AudioPath", out var audioPath))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} AudioPath is not a valid string!");
                    return;
                }

                if (!args.TryGetValue<float>("MaxAudibleDistance", out var maxAudibleDistance))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} MaxAudibleDistance is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("MinAudibleDistance", out var minAudibleDistance))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} MinAudibleDistance is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("SoundVolume", out var volume))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} SoundVolume is not a valid float!");
                    return;
                }

                Trigger = trigger;
                AudioPath = audioPath;
                MaxAudibleDistance = maxAudibleDistance;
                MinAudibleDistance = minAudibleDistance;
                Volume = volume;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || SummonedCustomItem == null)
                return;
                
            switch (eventArgs)
            {
                case PlayerInteractedDoorEventArgs playerInteractedDoor when HasFlagFast(Trigger, TriggerOn.OnDoorInteracted):
                    AudioIntegration.Play(SummonedCustomItem, playerInteractedDoor.Player.Position);
                    break;

                case PlayerShotWeaponEventArgs playerShotWeapon when HasFlagFast(Trigger, TriggerOn.OnShot):
                    AudioIntegration.Play(SummonedCustomItem, playerShotWeapon.Player.Position);
                    break;

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnUse):
                    AudioIntegration.Play(SummonedCustomItem, playerUsedItem.Player.Position);
                    break;

                case PlayerReloadedWeaponEventArgs playerReloadedWeapon when HasFlagFast(Trigger, TriggerOn.OnReload):
                    AudioIntegration.Play(SummonedCustomItem, playerReloadedWeapon.Player.Position);
                    break;

                case PlayerChangedItemEventArgs playerChangedItem when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    AudioIntegration.Play(SummonedCustomItem, playerChangedItem.Player.Position);
                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    AudioIntegration.Play(SummonedCustomItem, playerPickedUpItem.Player.Position);
                    break;

                case PlayerDroppedItemEventArgs playerDroppedItem when HasFlagFast(Trigger, TriggerOn.OnDropped):
                    AudioIntegration.Play(SummonedCustomItem, playerDroppedItem.Player.Position);
                    break;

                case PlayerDeathEventArgs playerDeath when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    AudioIntegration.Play(SummonedCustomItem, playerDeath.Player.Position);
                    break;
                
                case PlayerHurtEventArgs playerHurt when HasFlagFast(Trigger, TriggerOn.OnHurt):
                    AudioIntegration.Play(SummonedCustomItem, playerHurt.Player.Position);
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