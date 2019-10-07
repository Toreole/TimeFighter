using UnityEngine;
using System.Collections;

namespace Game
{
    public static partial class Util
    {
        public const float g = 9.81f;
        public const float g2 = 19.62f;
        public static float Normalized(float f) => (f > 0)? 1f : (f < 0)? -1f : 0f;
        public static int NormalizeInt(float f) => (f > 0) ? 1 : (f < 0) ? -1 : 0;
        
        public static Vector2 RotateVector2D(Vector2 vector, float angle)
        {
            return Quaternion.AngleAxis(angle, Vector3.forward) * vector;
        }

        public static IEnumerator Delay(System.Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }

        public static void Delay(MonoBehaviour mb, System.Action action, float time)
        {
            mb.StartCoroutine(Delay(action, time));
        }

        public static void DelayPhysicsFrame(MonoBehaviour mb, System.Action action)
        {
            mb.StartCoroutine(DelayPhysicsFrame(action, 1));
        }
        public static void DelayPhysicsFrames(MonoBehaviour mb, System.Action action, int frames)
        {
            mb.StartCoroutine(DelayPhysicsFrame(action,  frames));
        }

        private static IEnumerator DelayPhysicsFrame(System.Action action, int frameCount)
        {
            var wait = new WaitForFixedUpdate();
            for (int i = 0; i < frameCount; i++)
                yield return wait;
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
