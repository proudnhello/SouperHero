using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeybinds : MonoBehaviour
{
    public static PlayerKeybinds Singleton;

    public InputActionAsset actions;
    public InputActionReference movement;
    public InputActionReference cursorMovement;
    public InputActionReference useSpoon;
    public InputActionReference drinkSoup;
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

    Dictionary<string, InputActionReference> _GetBindingByActionName;

    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }

        _GetBindingByActionName = new()
        {
            { movement.action.name, movement},
            { cursorMovement.action.name, cursorMovement },
            { useSpoon.action.name, useSpoon },
            { drinkSoup.action.name, drinkSoup },
            { dash.action.name, dash },
            { interact.action.name, interact },
            { inventory.action.name, inventory },
            { pause.action.name, pause },
            { cycleSpoonLeft.action.name, cycleSpoonLeft },
            { cycleSpoonRight.action.name, cycleSpoonRight },
            { bowl1.action.name, bowl1 },
            { bowl2.action.name, bowl2 },
            { bowl3.action.name, bowl3 },
            { bowl4.action.name, bowl4 }
        };
    }

    public string GetBindingByActionName(string key)
    {
        if (_GetBindingByActionName.ContainsKey(key)) return _GetBindingByActionName[key].action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        return "";
    }
}
