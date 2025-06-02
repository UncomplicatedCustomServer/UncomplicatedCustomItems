using UnityEngine;

namespace UncomplicatedCustomItems.API.Extensions
{
    public static class Vector3Extensions
    {
        public static string ToHexColor(this Vector3 colorVector)
        {
            float rFloat = Mathf.Clamp01(colorVector.x);
            float gFloat = Mathf.Clamp01(colorVector.y);
            float bFloat = Mathf.Clamp01(colorVector.z);

            int r = (int)(rFloat * 255);
            int g = (int)(gFloat * 255);
            int b = (int)(bFloat * 255);

            return string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }
    }
}
