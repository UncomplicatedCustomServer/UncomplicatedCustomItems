using UnityEngine;

namespace UncomplicatedCustomItems.API.Interfaces.SpecificData
{
    public interface IFlashlightData : IData
    {
        public abstract string HexColor { get; set; }
        public abstract LightType LightType { get; set; }
        public abstract float Intensity { get; set; }
        public abstract LightShadows ShadowType { get; set; }
        public abstract float ShadowStrength { get; set; }
        public abstract float Range { get; set; }
        public abstract float SpotLightAngle { get; set; }
        public abstract LightShape Shape { get; set; }
    }
}
