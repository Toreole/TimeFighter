using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

namespace Game.Controller.Input
{
    public class InputTester : MonoBehaviour
    {
        public string[] inputs;
        public string[] buttons;

        public XInputDotNetAdapter adapt;

        private void Start()
        {
            var x = InputManager.GetControlScheme(PlayerID.One).GetActionLookupTable();
            foreach(KeyValuePair<string, InputAction> action in x)
            {
                print(action.Key + " -- " + action.Value.Description);
            }

#if UNITY_STANDALONE_WIN && ENABLE_X_INPUT 
            //var vibr = new GamepadVibration
            //{
            //    LeftMotor = 1f,
            //    RightMotor = 1f
            //}; 
            //adapt.SetVibration(vibr, GamepadIndex.GamepadOne);
            //yield return new WaitForSeconds(1f);
            //var v = adapt.GetVibration(GamepadIndex.GamepadOne);
            //v.RightMotor = 0;
            //v.LeftMotor = 0;
            //adapt.SetVibration(v, GamepadIndex.GamepadOne);

#endif 
        }

        private void Update()
        { 
            foreach(var input in inputs)
            {
                var ax = InputManager.GetAxis(input);
                if (Mathf.Abs(ax) > 0.01f)
                    print(input + ": " + ax.ToString("0.000"));
            }
            foreach(var a in buttons)
            {
                if(InputManager.GetButtonDown(a))
                {
                    print(a); 
                }
            }
        }

        /* 
        Array keys;

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
        }
        */

    }
}
