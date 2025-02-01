using CameraShaking;
using Exiled.API.Structs;
using System.Collections.Generic;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface IThrowableData : IData
    {
        public abstract float PinPullTime { get; set; }

        public abstract bool Repickable { get; set; }
    }
}
