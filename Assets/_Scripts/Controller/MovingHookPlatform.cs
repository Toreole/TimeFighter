using UnityEngine;
using System.Collections;
using Game.Controller;

namespace Game
{
    [AddComponentMenu("Hookables/Moving Anchor")]
    public class MovingHookPlatform : HookAnchor
    {
        float xOffset = 0;
        float maxOffset = 4f;
        float speed = 1.5f;
        int direction = 1;

        private void Update()
        {
            var addOff = direction * Time.deltaTime * speed;
            xOffset += addOff;
            if (xOffset > maxOffset || xOffset < -maxOffset)
                direction *= -1;
            transform.Translate(new Vector3(addOff, 0));
        }
    }
}