using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.Elements.SpecificData
{
    public class ThrowableData : Data, IThrowableData
    {
        public float PinPullTime { get; set; } = 1f;

        public bool Repickable { get; set; } = false;
    }
}
