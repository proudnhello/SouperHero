using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeybinds : MonoBehaviour
{
    public static PlayerKeybinds Singleton;

    public InputActionReference useSpoon;
    public InputActionReference dash;
    public InputActionReference interact;
    public InputActionReference inventory;
    public InputActionReference pause;

    public InputActionReference cycleSpoonLeft;
    public InputActionReference cycleSpoonRight;
    public InputActionReference bowl1;
    public InputActionReference bowl2;
    public InputActionReference bowl3;
    public InputActionReference bowl4;

    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
    }
}
