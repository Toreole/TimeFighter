using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybOrController : MonoBehaviour
{
    public Sprite controller;
    public Sprite keyb;
    public UnityEngine.UI.Image image;
    // Update is called once per frame
    void Update()
    {
        //TODO: handle this with the InputSystem.
        //image.sprite = InputManager.PreferController ? controller : keyb; //yeet
    }
}
