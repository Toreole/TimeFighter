using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

namespace Game.Controller.Input
{
    public class UseCorrectControllerInput : MonoBehaviour
    {
        // Only Windows builds can use Controller Input because im totally fucking upset at how it doesnt work otherwise
        void Awake()
        {
            //enable only after adding the correct shit
            GetComponent<InputManager>().enabled = true;
#if UNITY_STANDALONE_WIN
            gameObject.AddComponent<XInputDotNetAdapter>();
#endif
            Destroy(this);
        }

    }
}