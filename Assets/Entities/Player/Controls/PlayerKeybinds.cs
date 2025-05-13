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

    public InputActionReference cycleSpoon;
    public InputActionReference cycleSpoonLeft;
    public InputActionReference cycleSpoonRight;
    public InputActionReference soup_1;
    public InputActionReference soup_2;
    public InputActionReference soup_3;
    public InputActionReference soup_4;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
    }
}
