using UnityEngine;
using System;

namespace Game
{
    public static class Util
    {
        public static float Normalized(float f) => f / Mathf.Abs(f);
        public static int NormalizeInt(float f) => (int)(f / Mathf.Abs(f));
    }
}
