using UnityEngine;
using System;

namespace Game
{
    public static class Util
    {
        public static float Normalized(float f) => (f > 0)? 1f : (f < 0)? -1f : 0f;
        public static int NormalizeInt(float f) => (f > 0) ? 1 : (f < 0) ? -1 : 0;
    }
}
