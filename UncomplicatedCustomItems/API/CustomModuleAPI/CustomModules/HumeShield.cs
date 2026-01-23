using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class HumeShield : CustomModuleBase
    {
        public override string Name => "HumeShield";
        public override List<string> RequiredArguments => 
        [
            "RegenRate",
            "MaxHumeShield",
            "RegenCoolDown",
            "Trigger"
        ];
        
        public float RegenRate { get; set; }
        public float MaxHumeShield { get; set; }
        public float RegenCoolDown { get; set; }
        public TriggerOn Trigger { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            base.OnAdded(item);
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<float>("RegenRate", out var regenRate))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RegenRate is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("MaxHumeShield", out var maxHumeShield))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} MaxHumeShield is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("RegenCoolDown", out var regenCoolDown))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} RegenCoolDown is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<TriggerOn>("Trigger", out var trigger))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Trigger is not a valid enum value! {string.Join(", ", Enum.GetNames(typeof(TriggerOn)))}");
                    return;
                }

                RegenRate = regenRate;
                MaxHumeShield = maxHumeShield;
                RegenCoolDown = regenCoolDown;
                Trigger = trigger;
            }
        }

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            switch (eventArgs)
            {
                case PlayerDroppedItemEventArgs playerDroppedItem:
                    playerDroppedItem.Player.MaxHumeShield = 0;
                    playerDroppedItem.Player.HumeShieldRegenCooldown = 0;
                    playerDroppedItem.Player.HumeShieldRegenRate = 0;
                    break;

                case PlayerPickedUpArmorEventArgs playerPickedUpArmor when HasFlagFast(Trigger, TriggerOn.OnAdded):
                    playerPickedUpArmor.Player.MaxHumeShield = MaxHumeShield;
                    playerPickedUpArmor.Player.HumeShieldRegenCooldown = RegenCoolDown;
                    playerPickedUpArmor.Player.HumeShieldRegenRate = RegenRate;
                    break;

                case PlayerChangedItemEventArgs playerChangedItem when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    playerChangedItem.Player.MaxHumeShield = MaxHumeShield;
                    playerChangedItem.Player.HumeShieldRegenCooldown = RegenCoolDown;
                    playerChangedItem.Player.HumeShieldRegenRate = RegenRate;
                    break;

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnChangedItem):
                    playerUsedItem.Player.MaxHumeShield = MaxHumeShield;
                    playerUsedItem.Player.HumeShieldRegenCooldown = RegenCoolDown;
                    playerUsedItem.Player.HumeShieldRegenRate = RegenRate;
                    break;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.PickedUpArmor += Run;
            PlayerEvents.ChangedItem += Run;
            PlayerEvents.UsedItem += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.PickedUpArmor -= Run;
            PlayerEvents.ChangedItem -= Run;
            PlayerEvents.UsedItem -= Run;
        }
    }
}