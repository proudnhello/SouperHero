using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeybinds : MonoBehaviour
{
    public static PlayerKeybinds Singleton { get; private set; }

    public InputActionReference movement;
    public InputActionReference attack;
    public InputActionReference dash;
    public InputActionReference interact;
    public InputActionReference zoomOut;

    public InputActionReference cycleSpoon;
    public InputActionReference bowl_1;
    public InputActionReference bowl_2;
    public InputActionReference bowl_3;
    public InputActionReference bowl_4;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
    }
}
