using UncomplicatedCustomItems.Interfaces.FlagSettings;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class CustomGravitySettings : ICustomGravitySettings
    {
        public Vector3? Gravity { get; set; } = new(1, -19.60f, 1);
    }
}