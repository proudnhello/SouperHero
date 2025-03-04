using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>(); //This is returning null
    }
}
