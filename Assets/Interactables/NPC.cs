using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    [Header("NPC")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private bool repeatable = true; // if the NPC can be interacted with multiple times
    private int _interactCount = 0; // how many times the NPC has been interacted with
    public override void Interact()
    {
        if(!repeatable && _interactCount > 0)
        {
            // if the NPC is not repeatable and has already been interacted with, do nothing
            return;
        }
        if (CanInteract())
        {
            // Trigger the dialogue
            dialogueTrigger.TriggerDialogue();
            _interactCount++;
        }
    }

}
