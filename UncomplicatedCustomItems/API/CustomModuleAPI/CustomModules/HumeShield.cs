using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules.Enums;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class HumeShield : CustomModuleBase
    {
        public override string Name => "HumeShield";
        
        public float RegenRate { get; set; }
        public float MaxHumeShield { get; set; }
        public float RegenCoolDown { get; set; }
        public TriggerOn Trigger { get; set; }

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

                case PlayerUsedItemEventArgs playerUsedItem when HasFlagFast(Trigger, TriggerOn.OnUse):
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