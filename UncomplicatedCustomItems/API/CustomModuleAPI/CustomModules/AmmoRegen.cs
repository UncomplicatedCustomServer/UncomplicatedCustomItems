using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class AmmoRegen : CustomModuleBase
    {
        public override string Name => "AmmoRegen";
        public override List<string> RequiredArguments =>
        [
            "RegenDelay",
            "RegenInterval",
            "AmmoPerInterval",
        ];

        public float RegenDelay { get; set; }
        public float RegenInterval { get; set; }
        public int AmmoPerInterval { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            if (CustomItem == null)
                return;

            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<float>("RegenDelay", out var regenDelay))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RegenDelay is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("RegenInterval", out var regenInterval))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RegenInterval is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<int>("AmmoPerInterval", out var ammoPerInterval))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} AmmoPerInterval is not a valid int!");
                    return;
                }

                RegenDelay = regenDelay;
                RegenInterval = regenInterval;
                AmmoPerInterval = ammoPerInterval;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;

            if (eventArgs is PlayerShotWeaponEventArgs eve)
            {
                if (!Utilities.TryGetSummonedCustomItem(eve.FirearmItem.Serial, out var item))
                    return;

                item?.PauseAmmoRegen(eve.FirearmItem, this.RegenDelay);
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