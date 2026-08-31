using System;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class SingleFire : CustomModuleBase
    {
        public override string Name => "SingleFire";

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerReloadingWeaponEventArgs playerReloading)
            {
                if (playerReloading.FirearmItem.ChamberedAmmo >= 1)
                    playerReloading.IsAllowed = false;
            }

            if (eventArgs is PlayerReloadedWeaponEventArgs playerReloaded)
            {
                playerReloaded.FirearmItem.StoredAmmo = 0;
                playerReloaded.FirearmItem.ChamberedAmmo = 1;
                playerReloaded.FirearmItem.Cocked = true;
                playerReloaded.FirearmItem.BoltLocked = false;
                if (playerReloaded.FirearmItem.ActionModule is AutomaticActionModule actionModule)
                {
                    actionModule._serverQueuedRequests.Clear();
                    actionModule.ServerResync();
                }

                ushort currentAmmo = playerReloaded.Player.Ammo[playerReloaded.FirearmItem.AmmoType];
                playerReloaded.Player.SetAmmo(playerReloaded.FirearmItem.AmmoType, currentAmmo > 0 ? (ushort)(currentAmmo - 1) : (ushort)0);
            }

            if (eventArgs is PlayerChangedItemEventArgs playerChangedItem)
            {
                if (playerChangedItem.NewItem is FirearmItem item && item.MagazineControllerModule is MagazineModule magazine)
                {
                    if (magazine.AmmoStored > 1)
                    {
                        magazine.AmmoStored = 1;
                        magazine.ServerResyncData();
                    }
                }
            }

            if (eventArgs is PlayerShotWeaponEventArgs ev)
            {
                if (ev.FirearmItem.MagazineControllerModule is MagazineModule magazine)
                    magazine.AmmoStored = 0;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ReloadedWeapon += Run;
            PlayerEvents.ReloadingWeapon += Run;
            PlayerEvents.ChangedItem += Run;
            PlayerEvents.ShotWeapon += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ReloadedWeapon -= Run;
            PlayerEvents.ReloadingWeapon -= Run;
            PlayerEvents.ChangedItem -= Run;
            PlayerEvents.ShotWeapon -= Run;
        }
    }
}