using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    [Header("NPC")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private bool repeatable = true; // if the NPC can be interacted with multiple times

    private void Awake()
    {
        base.Awake();
        //SetInteractable(true);
    }
    public override void Interact()
    {

        if (CanInteract())
        {
            Debug.Log("Interacting with NPC: " + gameObject.name);
            // Trigger the dialogue
            if (dialogueTrigger != null)
            {
                dialogueTrigger.TriggerDialogue();
            }
            else
            {
                Debug.LogWarning("DialogueTrigger is not assigned for NPC: " + gameObject.name);
            }
            
            if(!repeatable)
            {
                SetInteractable(false);
            }
        }
    }

    public override void SetInteractable(bool value)
    {
        // set whether or not the interactable can be interacted with
        canInteract = value;
        if (value)
        {
            SetHighlighted(true);
        }
        else
        {
            SetHighlighted(false);
        }
    }
    // public override void SetHighlighted(bool isHighlighted)
    // {
    //     // set the interactable prompt to be active or not
    //     interactableSpriteRenderer.material.SetFloat(_OutlineThickness, isHighlighted ? 1 : 0);
    // }

}
