using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

namespace Game.Controller.Input
{
    public class UseCorrectControllerInput : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
#if UNITY_STANDALONE_WIN
            gameObject.AddComponent<XInputDotNetAdapter>();
#endif
            Destroy(this);
        }

    }
}