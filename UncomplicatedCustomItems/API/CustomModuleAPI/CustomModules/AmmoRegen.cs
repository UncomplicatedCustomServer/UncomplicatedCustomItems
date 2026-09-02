using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class AmmoRegen : CustomModuleBase
    {
        public override string Name => "AmmoRegen";

        public float RegenDelay { get; set; }
        public float RegenInterval { get; set; }
        public int AmmoPerInterval { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;

            if (eventArgs is PlayerShotWeaponEventArgs eve && eve.FirearmItem != null)
            {
                if (!Utilities.TryGetSummonedCustomItem(eve.FirearmItem.Serial, out var item))
                    return;

                item?.PauseAmmoRegen(eve.FirearmItem, RegenDelay);
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