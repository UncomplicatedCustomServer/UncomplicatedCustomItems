using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class ThrowableData : Data, IThrowableData
    {
        public float PinPullTime { get; set; } = 1f;

        public bool Repickable { get; set; } = false;

        public bool ExplodeOnCollision { get; set; } = true;

        public float FuseTime { get; set; } = 3f;
    }
}
