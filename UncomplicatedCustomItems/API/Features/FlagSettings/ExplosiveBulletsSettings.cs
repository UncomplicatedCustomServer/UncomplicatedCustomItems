using System.ComponentModel;
using UncomplicatedCustomItems.Interfaces;

namespace UncomplicatedCustomItems.API.Features
{
    public class ExplosiveBulletsSettings : IExplosiveBulletsSettings
    {
        public float? DamageRadius { get; set; } = 1f;
    }
}