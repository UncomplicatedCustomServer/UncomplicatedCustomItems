using System;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Effect : CustomModuleBase
    {
        public override string Name => "Effect";

        public string EffectName { get; set; } = string.Empty;
        public byte Intensity { get; set; } = 1;
        public float Duration { get; set; }
        public bool AddDurationIfActive { get; set; }
        public bool ClearOnUnequip { get; set; }
        public TriggerOn Trigger { get; set; }

        [YamlIgnore]
        private StatusEffectBase? StatusEffect { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            if (!string.IsNullOrEmpty(EffectName) && Server.Host != null)
            {
                if (Server.Host.TryGetEffect(EffectName, out var effect))
                {                    
                    StatusEffect = effect;
                }
                else
                    LogManager.Warn($"[Effect] Could not find effect '{EffectName}'.");
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || StatusEffect == null)
                return;
                
            float applyDuration = Duration >= 0 ? Duration : float.MaxValue;

            switch (eventArgs)
            {
                case PlayerShotWeaponEventArgs playerShotWeapon when HasFlagFast(Trigger, TriggerOn.OnShot):
                    playerShotWeapon.Player.EnableEffect(StatusEffect, Intensity, applyDuration, AddDurationIfActive);
                    break;

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnUse):
                    playerUsedItem.Player.EnableEffect(StatusEffect, Intensity, applyDuration, AddDurationIfActive);
                    break;

                case PlayerChangedItemEventArgs playerChangedItem:
                    if (ClearOnUnequip)
                        playerChangedItem.Player.DisableEffect(StatusEffect);

                    if (HasFlagFast(Trigger, TriggerOn.OnChangedItem))
                        playerChangedItem.Player.EnableEffect(StatusEffect, Intensity, applyDuration, AddDurationIfActive);

                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    playerPickedUpItem.Player.EnableEffect(StatusEffect, Intensity, applyDuration, AddDurationIfActive);
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
    }
}