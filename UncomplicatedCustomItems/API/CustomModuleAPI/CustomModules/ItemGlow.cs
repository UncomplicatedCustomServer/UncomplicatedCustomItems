using System.Collections.Generic;
using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Events;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class ItemGlow : CustomModuleBase
    {
        public override string Name => "ItemGlow";
        public override List<string> RequiredArguments =>
        [
            "GlowColor",
            "Intensity",
            "Range",
        ];

        public float Intensity { get; set; }
        public float Range { get; set; }
        public Color Color { get; set; }

        public override void OnAdded(SummonedCustomItem item)
        {
            foreach (Dictionary<object, object> args in Arguments)
            {
                if (!args.TryGetValue<string>("GlowColor", out var GlowColor))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} GlowColor is not a valid string!");
                    return;
                }

                if (!ColorUtility.TryParseHtmlString(GlowColor, out var color))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} GlowColor is not a valid Hexcode!");
                    return;
                }

                if (!args.TryGetValue<float>("Intensity", out var intensity))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Intensity is not a valid float!");
                    return;
                }

                if (!args.TryGetValue<float>("Range", out var range))
                {
                    LogManager.Warn($"{CustomItem.Name} - {CustomItem.Id} Range is not a valid float!");
                    return;
                }             

                Color = color;
                Intensity = intensity;
                Range = range;   
            }
        }

        public void Run(ItemPickupBase pickupBase)
        {
            Pickup pickup = Pickup.Get(pickupBase);
            if (!Utilities.TryGetSummonedCustomItem(pickup.Serial, out var item) || item.CustomItem != CustomItem)
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