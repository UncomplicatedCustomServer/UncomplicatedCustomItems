using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.Events;
using UnityEngine;
using YamlDotNet.Serialization;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ItemGlow : CustomModuleBase
    {
        public override string Name => "ItemGlow";

        public string GlowColor { get; set; } = "#FFFFFF";
        public float Intensity { get; set; } = 1f;
        public float Range { get; set; } = 5f;

        [YamlIgnore]
        public Color Color { get; set; } = Color.white;

        public override void OnAdded(SummonedCustomItem item)
        {
            if (ColorUtility.TryParseHtmlString(GlowColor, out var color))
            {
                Color = color;                
            }
            else
                LogManager.Warn($"[ItemGlow] GlowColor '{GlowColor}' is not a valid hex code.");
        }

        public void Run(ItemPickupBase pickupBase)
        {
            Pickup pickup = Pickup.Get(pickupBase);
            if (pickup == null || !Utilities.TryGetSummonedCustomItem(pickup.Serial, out var item) || item?.CustomItem != CustomItem)
                return;
            
            LightSourceToy light = LightSourceToy.Create(pickup.Position);
            light.Color = Color;
            light.Intensity = Intensity;
            light.Range = Range;
            light.ShadowType = LightShadows.None;
            light.Base.transform.SetParent(pickup.Base.transform, true);

            light.Position += Vector3.up * 0.1f;
            LogManager.Debug($"Item Light spawned at position: {light.Position}");
            PlayerHandler.ActiveLights[pickup] = light;
        }

        public override void RegisterEvents()
        {
            ItemPickupBase.OnPickupAdded += Run;
        }

        public override void UnregisterEvents()
        {
            ItemPickupBase.OnPickupAdded -= Run;
        }
    }
}