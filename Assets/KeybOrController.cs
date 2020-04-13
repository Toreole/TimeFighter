using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybOrController : MonoBehaviour
{
    public Sprite controller;
    public Sprite keyb;
    public UnityEngine.UI.Image image;
    public PlayerInput input;

    private void Start()
    {
        input.onControlsChanged += ControlChange;
        image.sprite = input.currentControlScheme == "Gamepad" ? controller : keyb;
    }

    private void ControlChange(PlayerInput obj)
    {
        image.sprite = obj.currentControlScheme == "Gamepad" ? controller : keyb;
    }
}