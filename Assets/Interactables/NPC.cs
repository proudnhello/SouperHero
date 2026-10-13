using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    [Header("NPC")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private bool repeatable = true; // if the NPC can be interacted with multiple times
    [SerializeField] SpriteRenderer sprite;

    private bool isSpeaking = false; // if the NPC is currently speaking
    EntityRenderer entityRenderer;

    private new void Awake()
    {
        base.Awake();
        entityRenderer = new(null, sprite);
    }

    IEnumerator DamageAnim;
    public void TakeDamange() 
    {
        if (DamageAnim != null) StopCoroutine(DamageAnim);
        StartCoroutine(DamageAnim = entityRenderer.TakeDamageAnimation());
    }

    public bool testSpeaking()
    {
        return isSpeaking;
    }

    public override void Interact()
    {

        if (CanInteract())
        {
            // Trigger the dialogue
            if (dialogueTrigger != null)
            {
                if (isSpeaking)
                {
                    DialogueManager.Singleton.DisplayNextSentence(); // Display next sentence if already speaking

                    // Check if dialogue has finished
                    if (!DialogueManager.Singleton.IsDialogueActive())
                    {
                        isSpeaking = false; // Reset speaking state when dialogue ends
                    }
                }
                else
                {
                    isSpeaking = true; // Set speaking state to true
                    dialogueTrigger.TriggerDialogue();
                    CameraMover.Singleton.ActiveNPCPosition = transform.position;
                }
            }
            else
            {
                Debug.LogWarning("DialogueTrigger is not assigned for NPC: " + gameObject.name);
            }

            if (!repeatable)
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
