using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class UseCorrectControllerInput : MonoBehaviour
{
    [SerializeField]
    GenericGamepadStateAdapter genericApdater;
    [SerializeField]
    GenericGamepadProfileSelector genericSelector;
    [SerializeField]
    XInputDotNetAdapter xInputAdapter;

        // Start is called before the first frame update
    void Awake()
    {
#if UNITY_STANDALONE_WIN
        Destroy(genericSelector);
        Destroy(genericApdater);
#else
        Destroy(xInputAdapter);
#endif
        Destroy(this);
    }
    
}
