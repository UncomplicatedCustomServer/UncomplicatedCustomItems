using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DestroyOn : CustomModuleBase
    {
        public override string Name => "DestroyOn";

        public TriggerOn Trigger { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;

            SummonedCustomItem? toDestroy = null;

            switch (eventArgs)
            {
                case PlayerShotWeaponEventArgs shot when HasFlagFast(Trigger, TriggerOn.OnShot):
                    if (Utilities.TryGetSummonedCustomItem(shot.FirearmItem.Serial, out var shotSci))
                        toDestroy = shotSci;

                    break;

                case PlayerUsedItemEventArgs used when HasFlagFast(Trigger, TriggerOn.OnUse):
                    if (Utilities.TryGetSummonedCustomItem(used.UsableItem.Serial, out var usedSci))
                        toDestroy = usedSci;

                    break;

                case PlayerReloadedWeaponEventArgs reloaded when HasFlagFast(Trigger, TriggerOn.OnReload):
                    if (Utilities.TryGetSummonedCustomItem(reloaded.FirearmItem.Serial, out var reloadSci))
                        toDestroy = reloadSci;

                    break;

                case PlayerChangedItemEventArgs changed when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    if (changed.NewItem != null && Utilities.TryGetSummonedCustomItem(changed.NewItem.Serial, out var newSci))
                    {
                        toDestroy = newSci;
                    }
                    else if (changed.OldItem != null && Utilities.TryGetSummonedCustomItem(changed.OldItem.Serial, out var oldSci))
                    {
                        toDestroy = oldSci;
                    }

                    break;

                case PlayerPickedUpItemEventArgs picked when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    if (Utilities.TryGetSummonedCustomItem(picked.Item.Serial, out var pickedSci))
                        toDestroy = pickedSci;

                    break;

                case PlayerDroppedItemEventArgs dropped when HasFlagFast(Trigger, TriggerOn.OnDropped):
                    if (Utilities.TryGetSummonedCustomItem(dropped.Pickup.Serial, out var droppedSci))
                        toDestroy = droppedSci;

                    break;

                case PlayerDeathEventArgs death when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    Player? deathTarget = ResolveDeathTarget(death.Player, death.Attacker);
                    if (deathTarget?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(deathTarget.CurrentItem.Serial, out var deathSci))
                        toDestroy = deathSci;

                    break;

                case PlayerDyingEventArgs dying when HasFlagFast(Trigger, TriggerOn.OnDeath):
                    Player? dyingTarget = ResolveDeathTarget(dying.Player, dying.Attacker);
                    if (dyingTarget?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(dyingTarget.CurrentItem.Serial, out var dyingSci))
                        toDestroy = dyingSci;

                    break;

                case PlayerHurtEventArgs hurt when HasFlagFast(Trigger, TriggerOn.OnHurt):
                    Player? hurtTarget = ResolveHurtTarget(hurt);
                    if (hurtTarget?.CurrentItem != null && Utilities.TryGetSummonedCustomItem(hurtTarget.CurrentItem.Serial, out var hurtSci))
                        toDestroy = hurtSci;

                    break;

                case PlayerInteractedDoorEventArgs door when HasFlagFast(Trigger, TriggerOn.OnDoorInteracted):
                    if (door.Player.CurrentItem != null && Utilities.TryGetSummonedCustomItem(door.Player.CurrentItem.Serial, out var doorSci))
                        toDestroy = doorSci;

                    break;

                case PlayerInspectedItemEventArgs inspected when HasFlagFast(Trigger, TriggerOn.OnInspected):
                    if (Utilities.TryGetSummonedCustomItem(inspected.Item.Serial, out var inspectedSci))
                        toDestroy = inspectedSci;

                    break;

                default:
                    return;
            }

            toDestroy?.Destroy();
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
