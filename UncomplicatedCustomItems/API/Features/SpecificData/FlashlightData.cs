using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    public class FlashlightData : Data, IFlashlightData
    {
        public virtual string HexColor { get; set; } = string.Empty;

        [Description("Available values are: Spot, Directional, Point, Area, Rectangle, Disc")]
        public virtual LightType LightType { get; set; } = LightType.Spot;

        public virtual float Intensity { get; set; } = 1f;

        [Description("Available values are: None, Hard, Soft")]
        public virtual LightShadows ShadowType { get; set; } = LightShadows.Soft;

        public virtual float ShadowStrength { get; set; } = 1f;

        [Description("Sets the range in Meters.")]
        public virtual float Range { get; set; } = 4f;

        public virtual float SpotLightAngle { get; set; } = 90f;

#pragma warning disable CS0618 // Type or member is obsolete
        [Description("Sets or gets the shape of the light if its a spotlight")]
        public virtual LightShape Shape { get; set; } = LightShape.Cone;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}


