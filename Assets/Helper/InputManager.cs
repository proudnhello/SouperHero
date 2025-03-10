using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Input manager finds the current input system being used
//In theory, should swap between UI and Player input when pausing the game
//Right now the input is simply disabled. This is controlled in the GameManager
public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
}
