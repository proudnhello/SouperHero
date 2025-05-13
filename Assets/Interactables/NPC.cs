using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    [Header("NPC")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private bool repeatable = true; // if the NPC can be interacted with multiple times

    protected override void Awake()
    {
        base.Awake();
    }
    public override void Interact()
    {

        if (CanInteract())
        {
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
}
