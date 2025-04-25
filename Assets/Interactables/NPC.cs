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
        // set the interactable to be highlighted
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

}
