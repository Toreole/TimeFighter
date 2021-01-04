using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugUtil : MonoBehaviour
{
    public static bool ShowDebugGUI {get; private set;} = false;
    public static readonly GUIStyle boldLabel = new GUIStyle();

    [SerializeField]
    private InputAction debugUIToggleAction;
    
    private void Start() 
    {
        boldLabel.fontStyle = FontStyle.Bold;
        if(debugUIToggleAction != null)
        {
            debugUIToggleAction.Enable();
            debugUIToggleAction.performed += ToggleGUI;
        }
    }

    private void ToggleGUI(InputAction.CallbackContext obj)
    {
        //the funny invert
        DebugUtil.ShowDebugGUI ^= true;
    }
}
