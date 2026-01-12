using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Effect : CustomModuleBase
    {
        public override string Name => "Effect";
        public override List<string> RequiredArguments =>
        [
            "EffectName",
            "Intensity",
            "Duration",
            "AddDurationIfActive",
            "ClearOnUnequip",
            "Trigger",
        ];

        public string EffectName { get; set; }
        public byte Intensity { get; set; }
        public float Duration { get; set; }
        public bool AddDurationIfActive { get; set; }
        public bool ClearOnUnequip { get; set; }
        public TriggerOn Trigger { get; set; }
        private StatusEffectBase effect { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("EffectName", out var EffectName))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} EffectName is not a valid string!");
                    return;
                }

                if (!Server.Host.TryGetEffect(EffectName, out var _effect))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} {EffectName} is not a valid Effect!");
                    return;
                }

                if (!args.TryGetValue<byte>("Intensity", out var intensity))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Intensity is not a valid byte!");
                    return;
                }

                if (!args.TryGetValue<float>("Duration", out var duration))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Duration is not a valid float!");
                    return;
                }


                if (!args.TryGetValue<bool>("AddDurationIfActive", out var addDurationIfActive))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} AddDurationIfActive is not a valid boolean");
                    return;
                }

                if (!args.TryGetValue<bool>("ClearOnUnequip", out var clearOnUnequip))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ClearOnUnequip is not a valid boolean");
                    return;
                }

                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                effect = _effect;
                Intensity = intensity;
                Duration = duration;
                AddDurationIfActive = addDurationIfActive;
                ClearOnUnequip = clearOnUnequip;
                Trigger = trigger;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            base.Run(eventArgs);
            switch (eventArgs)
            {
                case PlayerShotWeaponEventArgs playerShotWeapon when HasFlagFast(Trigger, TriggerOn.OnShot):
                    if (Duration >= 0)
                        playerShotWeapon.Player.EnableEffect(effect, Intensity, Duration, AddDurationIfActive);
                    else
                        playerShotWeapon.Player.EnableEffect(effect, Intensity, float.MaxValue, AddDurationIfActive);
                    break;

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnUse):
                    if (Duration >= 0)
                        playerUsedItem.Player.EnableEffect(effect, Intensity, Duration, AddDurationIfActive);
                    else
                        playerUsedItem.Player.EnableEffect(effect, Intensity, float.MaxValue, AddDurationIfActive);
                    break;

                case PlayerChangedItemEventArgs playerChangedItem:
                    if (ClearOnUnequip)
                        playerChangedItem.Player.DisableEffect(effect);

                    if (HasFlagFast(Trigger, TriggerOn.OnChangedItem))
                    {
                        if (Duration >= 0)
                            playerChangedItem.Player.EnableEffect(effect, Intensity, Duration, AddDurationIfActive);
                        else
                            playerChangedItem.Player.EnableEffect(effect, Intensity, float.MaxValue, AddDurationIfActive);
                    }
                    break;

                case PlayerPickedUpItemEventArgs playerPickedUpItem when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    if (Duration >= 0)
                        playerPickedUpItem.Player.EnableEffect(effect, Intensity, Duration, AddDurationIfActive);
                    else
                        playerPickedUpItem.Player.EnableEffect(effect, Intensity, float.MaxValue, AddDurationIfActive);
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