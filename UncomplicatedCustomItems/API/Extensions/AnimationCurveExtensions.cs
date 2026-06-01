// -----------------------------------------------------------------------
// <copyright file="AnimationCurveExtensions.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
// This file contains code originally licensed under CC BY-SA 3.0.
// Modifications and adaptations by UCSC are licensed under GNU AGPL v3.
// See LICENSE file for full GNU AGPL v3 license text.
// -----------------------------------------------------------------------

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