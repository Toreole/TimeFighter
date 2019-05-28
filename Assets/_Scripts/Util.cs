using UnityEngine;
using System.Collections;

namespace Game
{
    public static class Util
    {
        public static float Normalized(float f) => (f > 0)? 1f : (f < 0)? -1f : 0f;
        public static int NormalizeInt(float f) => (f > 0) ? 1 : (f < 0) ? -1 : 0;

        public static IEnumerator Delay(System.Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }

        /// <summary>
        /// Draw with default color = red
        /// </summary>
        public static void DrawCross(Vector2 pos, float width)
        {
            DrawCross(pos, width, Color.red);
        }
        /// <summary>
        /// Draw a cross (vertical and horizontal) at the position.
        /// </summary>
        public static void DrawCross(Vector2 pos, float width, Color color)
        {
            Vector2 start = pos - Vector2.right * width / 2f;
            Vector2 end = start + Vector2.right * width;
            Debug.DrawLine(start, end, color);

            start = pos - Vector2.up * width / 2f;
            end = start + Vector2.up * width;
            Debug.DrawLine(start, end, color);
        }
    }
}
