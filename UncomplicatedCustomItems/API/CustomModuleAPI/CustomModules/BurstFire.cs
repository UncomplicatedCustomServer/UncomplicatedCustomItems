using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using InventorySystem.Items.Firearms.Modules;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class BurstFire : CustomModuleBase
    {
        public override string Name => "BurstFire";

        public uint TotalFired { get; set; }
        public uint BurstAmount { get; set; }
        public bool ForceFire { get; set; }
        public float CoolDown { get; set; }
        public float TimingBetweenForcedShots { get; set; }

        public CoroutineHandle CooldownHandle { get; set; }
        public CoroutineHandle ForceFireHandle { get; set; }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs) || ForceFireHandle.IsRunning)
                return;

            if (eventArgs is PlayerShootingWeaponEventArgs ev)
            {
                if (TotalFired >= BurstAmount)
                {
                    if (!CooldownHandle.IsRunning)
                        CooldownHandle = Timing.RunCoroutine(ResetCooldownCoroutine());
                        
                    ev.IsAllowed = false;
                    return;
                }

                TotalFired++;
                if (ForceFire && !ForceFireHandle.IsRunning)
                    ForceFireHandle = Timing.RunCoroutine(ForceFireCoroutine(ev.FirearmItem));
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ShootingWeapon += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ShootingWeapon -= Run;
        }

        public IEnumerator<float> ResetCooldownCoroutine()
        {
            yield return Timing.WaitForSeconds(CoolDown);
            TotalFired = 0;
        }

        public IEnumerator<float> ForceFireCoroutine(FirearmItem item)
        {
            for (int i = 0; i < BurstAmount - 1; i++)
            {
                yield return Timing.WaitForSeconds(TimingBetweenForcedShots);
                if (item?.Base != null && item.Base.Owner != null && item.ActionModule is AutomaticActionModule actionModule && (item.StoredAmmo + item.ChamberedAmmo) >= 1)
                {
                    actionModule.ServerShoot(item.Base.Owner);
                }
            }
        }
    }
}