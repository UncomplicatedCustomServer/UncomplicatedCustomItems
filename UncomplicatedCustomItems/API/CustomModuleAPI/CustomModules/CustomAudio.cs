using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.Integrations;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class CustomAudio : CustomModuleBase
    {
        public override string Name => "CustomAudio";

        [YamlIgnore]
        public SummonedCustomItem? SummonedCustomItem { get; private set; }

        public TriggerOn Trigger { get; set; }
        public bool ParentToPlayer { get; set; } = false;
        public string AudioPath { get; set; } = string.Empty;
        public float MaxAudibleDistance { get; set; }
        public float MinAudibleDistance { get; set; }
        public float Volume { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            SummonedCustomItem = item;
            base.OnAdded(item);
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || SummonedCustomItem == null)
                return;
                
            switch (eventArgs)
            {
                case PlayerInteractedDoorEventArgs playerInteractedDoor when HasFlagFast(Trigger, TriggerOn.OnDoorInteracted):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerInteractedDoor.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerShotWeaponEventArgs playerShotWeapon when HasFlagFast(Trigger, TriggerOn.OnShot):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerShotWeapon.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnUse):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerUsedItem.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerReloadedWeaponEventArgs playerReloadedWeapon when HasFlagFast(Trigger, TriggerOn.OnReload):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerReloadedWeapon.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerChangedItemEventArgs playerChangedItem when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerChangedItem.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerPickedUpItem.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerDroppedItemEventArgs playerDroppedItem when HasFlagFast(Trigger, TriggerOn.OnDropped):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerDroppedItem.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;

                case PlayerDeathEventArgs playerDeath when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerDeath.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
                    break;
                
                case PlayerHurtEventArgs playerHurt when HasFlagFast(Trigger, TriggerOn.OnHurt):
                    if (!ParentToPlayer)
                    {
                        AudioIntegration.Play(SummonedCustomItem, playerHurt.Player.Position);
                        return;
                    }

                    AudioIntegration.Play(SummonedCustomItem);
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