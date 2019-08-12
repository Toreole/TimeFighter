using System;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

using UInput = UnityEngine.Input;

namespace Game.Controller.Input
{
    public class InputTester : MonoBehaviour
    {
        /*Array keys;

        public string[] controllerAxes;

        private void Start()
        {
            keys = Enum.GetValues(typeof(KeyCode));

            var sticks = UInput.GetJoystickNames();
            if(sticks.Length > 0)
            {
                for(int i = 0; i < sticks.Length; i++)
                {
                    var stick = sticks[i];
                    if (!string.IsNullOrEmpty(stick))
                    Debug.Log("Connected at " + i.ToString() + ": " + stick);
                }
            }
        }

        private void Update() 
        {
            foreach(KeyCode key in keys)
            {
                if(UInput.GetKeyDown(key))
                {
                    Debug.Log("Pressed: " + key.ToString());
                }
            }
            foreach(var axis in controllerAxes)
            {
                float t = UInput.GetAxis(axis);
                if (Mathf.Abs(t) > 0.1)
                {
                    Debug.Log(axis + ": " + t.ToString("0.000"));
                }
            }
        }*/


    }
}
