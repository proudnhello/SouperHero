using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Info")]
    protected string type;

    [Header("Interactable (Do Not Edit)")]
    [SerializeField] protected bool canInteract = true; //Always start object as interactable

    [Header("Make Sure is Set")]
    [SerializeField] public GameObject interactablePrompt;

    public abstract void Interact();

    public bool CanInteract()
    {  
        // return whether or not the interactable can be interacted with
        return canInteract;
    }

    public void SetInteractable(bool value)
    {
        // set whether or not the interactable can be interacted with
        canInteract = value;
    }

    public string GetInteractableType()
    {
        // return the type of the interactable
        return type;
    }

    public void SetInteractablePrompt(bool value)
    {
        // set the interactable prompt to be active or not
        interactablePrompt.SetActive(value);
    }
}
