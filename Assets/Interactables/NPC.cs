using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    [Header("NPC")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private bool repeatable = true; // if the NPC can be interacted with multiple times
    private int _interactCount = 0; // how many times the NPC has been interacted with

    private void Awake()
    {
        // set the interactable to be highlighted
        SetInteractable(true);
    }
    public override void Interact()
    {
        if(!repeatable && _interactCount > 0)
        {
            SetInteractable(false); // set the interactable to false so it can't be interacted with again
        }

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
            _interactCount++;
        }
    }

}
