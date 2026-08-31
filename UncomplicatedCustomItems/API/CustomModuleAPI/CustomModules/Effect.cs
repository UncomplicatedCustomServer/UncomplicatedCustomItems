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
                    LogManager.Warn($"Could not find effect '{EffectName}'.");
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;

            if (string.IsNullOrEmpty(EffectName))
                return;

            if (StatusEffect == null && Server.Host != null)
            {
                if (!Server.Host.TryGetEffect(EffectName, out var eff))
                {
                    LogManager.Warn($"Could not find effect '{EffectName}'.");
                    return;
                }
                StatusEffect = eff;
            }
            else if (StatusEffect == null)
            {
                LogManager.Warn($"Could not find effect '{EffectName}' - StatusEffect is null and Host is null.");
                return;
            }
                
            float applyDuration = Duration >= 0 ? Duration : float.MaxValue;

            Player? target;
            switch (eventArgs)
            {
                case PlayerShotWeaponEventArgs shot when HasFlagFast(Trigger, TriggerOn.OnShot):
                    target = shot.Player;
                    break;

                case PlayerItemUsageEffectsApplyingEventArgs usageApplying when HasFlagFast(Trigger, TriggerOn.OnUse):
                    target = usageApplying.Player;
                    break;

                case PlayerReloadedWeaponEventArgs reloaded when HasFlagFast(Trigger, TriggerOn.OnReload):
                    target = reloaded.Player;
                    break;

                case PlayerChangedItemEventArgs changed:
                    if (ClearOnUnequip)
                        changed.Player.DisableEffect(StatusEffect);

                    if (!HasFlagFast(Trigger, TriggerOn.OnChangedItem))
                        return;

                    target = changed.Player;
                    break;

                case PlayerPickedUpItemEventArgs picked when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    target = picked.Player;
                    break;

                case PlayerDroppedItemEventArgs dropped when HasFlagFast(Trigger, TriggerOn.OnDropped):
                    target = dropped.Player;
                    break;

                case PlayerDeathEventArgs death when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    target = ResolveDeathTarget(death.Player, death.Attacker);
                    break;

                case PlayerDyingEventArgs dying when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    target = ResolveDeathTarget(dying.Player, dying.Attacker);
                    break;

                case PlayerHurtEventArgs hurt when HasFlagFast(Trigger, TriggerOn.OnHurt):
                    target = ResolveHurtTarget(hurt);
                    break;

                case PlayerInteractedDoorEventArgs door when HasFlagFast(Trigger, TriggerOn.OnDoorInteracted):
                    target = door.Player;
                    break;

                case PlayerInspectedItemEventArgs inspected when HasFlagFast(Trigger, TriggerOn.OnInspected):
                    target = inspected.Player;
                    break;

                default:
                    return;
            }

            if (target == null)
                return;

            if (!target.TryGetEffect(EffectName, out var targetEffect) || targetEffect == null)
            {
                LogManager.Warn($"Could not find effect '{EffectName}' for player {target.Nickname}.");
                return;
            }

            if (!targetEffect.AllowEnabling && Intensity > targetEffect.Intensity)
            {
                targetEffect.ForceIntensity(Intensity);
                targetEffect.ServerChangeDuration(applyDuration, AddDurationIfActive);
                target.ReferenceHub.playerEffectsController.ServerSyncEffect(targetEffect);
                LogManager.Debug($"Force applied {EffectName} to {target.Nickname} Intensity={Intensity} Duration={applyDuration}");
            }
            else
            {
                target.EnableEffect(targetEffect, Intensity, applyDuration, AddDurationIfActive);
                LogManager.Debug($"Applied {EffectName} to {target.Nickname} Intensity={Intensity} Duration={applyDuration} AddDuration={AddDurationIfActive}");
            }
        }

        private Player? ResolveHurtTarget(PlayerHurtEventArgs ev)
        {
            if (ev.Player?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(ev.Player.CurrentItem.Serial, out var victimItem) && victimItem?.CustomItem == CustomItem)
                return ev.Player;

            if (ev.Attacker?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(ev.Attacker.CurrentItem.Serial, out var attackerItem) && attackerItem?.CustomItem == CustomItem)
                return ev.Attacker;

            return ev.Player;
        }

        private Player? ResolveDeathTarget(Player? victim, Player? attacker)
        {
            if (victim?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(victim.CurrentItem.Serial, out var victimItem) && victimItem?.CustomItem == CustomItem)
                return victim;

            if (attacker?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(attacker.CurrentItem.Serial, out var attackerItem) && attackerItem?.CustomItem == CustomItem)
                return attacker;

            return victim;
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ShotWeapon += Run;
            PlayerEvents.ItemUsageEffectsApplying += Run;
            PlayerEvents.ReloadedWeapon += Run;
            PlayerEvents.ChangedItem += Run;
            PlayerEvents.PickedUpItem += Run;
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.Death += Run;
            PlayerEvents.Dying += Run;
            PlayerEvents.Hurt += Run;
            PlayerEvents.InteractedDoor += Run;
            PlayerEvents.InspectedItem += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ShotWeapon -= Run;
            PlayerEvents.ItemUsageEffectsApplying -= Run;
            PlayerEvents.ReloadedWeapon -= Run;
            PlayerEvents.ChangedItem -= Run;
            PlayerEvents.PickedUpItem -= Run;
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.Death -= Run;
            PlayerEvents.Dying -= Run;
            PlayerEvents.Hurt -= Run;
            PlayerEvents.InteractedDoor -= Run;
            PlayerEvents.InspectedItem -= Run;
        }
    }
}