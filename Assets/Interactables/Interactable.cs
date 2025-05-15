using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected bool canInteract = true; //Always start object as interactable

    [Header("Make Sure is Set")]
    protected SpriteRenderer interactableSpriteRenderer;

    protected int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    protected virtual void Awake()
    {
        interactableSpriteRenderer = GetComponent<SpriteRenderer>();
        SetHighlighted(false);
    }

    public abstract void Interact();

    public bool CanInteract()
    {
        // return whether or not the interactable can be interacted with
        return canInteract;
    }

    public virtual void SetInteractable(bool value)
    {
        // set whether or not the interactable can be interacted with
        canInteract = value;
    }

    public virtual void SetHighlighted(bool isHighlighted)
    {
        // set the interactable prompt to be active or not
        if (interactableSpriteRenderer != null)
        {
            interactableSpriteRenderer.material.SetFloat(_OutlineThickness, isHighlighted ? 1 : 0);
        }
        else
        {
            Debug.LogWarning("Sprite renderer = null in GO");
        }
    }
}
