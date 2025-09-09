using UnityEngine;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class AnimationCurveExtensions
    {
        public static AnimationCurve Multiply(this AnimationCurve curve, float amount)
        {
            Keyframe[] keys = curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value *= amount;
            }
            curve.keys = keys;
            return curve;
        }
    }
}