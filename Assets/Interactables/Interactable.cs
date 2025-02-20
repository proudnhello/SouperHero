using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected bool canInteract = true; //Always start object as interactable

    [Header("Make Sure is Set")]
    protected SpriteRenderer interactableSpriteRenderer;
    [SerializeField] public TextMeshPro interactablePromptText;

    int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    void Awake()
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
        interactableSpriteRenderer.material.SetFloat(_OutlineThickness, isHighlighted ? 1 : 0);
    }
}
