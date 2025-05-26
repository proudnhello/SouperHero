using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;

    public void ResetALLBindings()
    {
        foreach (InputActionMap map in _inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }
}
