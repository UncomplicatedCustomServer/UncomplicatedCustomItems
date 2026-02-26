using System.Collections.Generic;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using MEC;
using LabApi.Features.Wrappers;
using InventorySystem.Items.Firearms.Modules;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class BurstFire : CustomModuleBase
    {
        public override string Name => "BurstFire";
        public override List<string> RequiredArguments =>
        [
            "BurstAmount",
            "CoolDown",
            "ForceFire",
            "TimingBetweenForcedShots"
        ];

        public CoroutineHandle CooldownHandle { get; set; }
        public CoroutineHandle ForceFireHandle { get; set; }
        public uint TotalFired { get; set; }
        public uint BurstAmount { get; set; }
        public bool ForceFire { get; set; }
        public float CoolDown { get; set; }
        public float TimingBetweenForcedShots { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<bool>("ForceFire", out var force))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} ForceFire is not a valid Boolean!");
                    return;
                }

                if (!args.TryGetValue<uint>("BurstAmount", out var burst))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} BurstAmount is not a valid UInt!");
                    return;
                }

                if (!args.TryGetValue<float>("CoolDown", out var cool))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} CoolDown is not a valid Float!");
                    return;
                }

                if (!args.TryGetValue<float>("TimingBetweenForcedShots", out var timing))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} TimingBetweenForcedShots is not a valid Float!");
                    return;
                }

                BurstAmount = burst;
                ForceFire = force;
                CoolDown = cool;
                TimingBetweenForcedShots = timing;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
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
                if (item.ActionModule is AutomaticActionModule actionModule)
                {
                    actionModule.ServerShoot(item.Base.Owner);
                }
            }
        }
    }
}