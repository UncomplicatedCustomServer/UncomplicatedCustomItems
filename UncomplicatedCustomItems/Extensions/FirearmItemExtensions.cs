using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomItems.Extensions
{
    public static class FirearmItemExtensions
    {
        public static bool IsAiming(this FirearmItem firearm) => firearm.Base.TryGetModule(out IAdsModule module) && module.AdsTarget;
        public static bool FlashLightStatus(this FirearmItem firearm) => firearm.Base.TryGetModule(out FlashlightAttachment module) && module.IsEnabled;
    }
}
