using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DieOn : CustomModuleBase
    {
        public override string Name => "DieOn";

        public string DeathMessage { get; set; } = string.Empty;
        public bool Vaporize { get; set; }
        public TriggerOn Trigger { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || CustomItem == null)
                return;

            Player? target;
            switch (eventArgs)
            {
                case PlayerShotWeaponEventArgs shot when HasFlagFast(Trigger, TriggerOn.OnShot):
                    target = shot.Player;
                    break;

                case PlayerUsedItemEventArgs used when HasFlagFast(Trigger, TriggerOn.OnUse):
                    target = used.Player;
                    break;

                case PlayerReloadedWeaponEventArgs reloaded when HasFlagFast(Trigger, TriggerOn.OnReload):
                    target = reloaded.Player;
                    break;

                case PlayerChangedItemEventArgs changed when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
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

            if (Vaporize)
            {
                target.Vaporize();
            }
            else
                target.Kill(DeathMessage.Replace("%name%", CustomItem.Name));
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
            PlayerEvents.UsedItem += Run;
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
            PlayerEvents.UsedItem -= Run;
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