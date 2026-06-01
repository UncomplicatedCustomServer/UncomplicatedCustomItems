using System;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Features.Manager;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class InfiniteAmmo : CustomModuleBase
    {
        public override string Name => "InfiniteAmmo";

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerShotWeaponEventArgs ev)
            {
                LogManager.Debug("Running InfiniteAmmo");
                if (ev.FirearmItem.AmmoContainerModule is MagazineModule magazine)
                    magazine.ServerSetInstanceAmmo(ev.FirearmItem.Serial, ev.FirearmItem.MaxAmmo);
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ShotWeapon += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ShotWeapon -= Run;
        }
    }
}