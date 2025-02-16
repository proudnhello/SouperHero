using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEnvInteraction : MonoBehaviour
{
    private Interactable currentInteractable = null;
    private int lastInteractionFrame = -1;

    private void Start()
    {
        PlayerEntityManager.Singleton.input.Player.Interact.started += Interact;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && interactable.CanInteract())
        {
            currentInteractable = interactable;
            interactable.SetInteractablePrompt(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
            interactable.SetInteractablePrompt(false);
        }
    }

    private int interactCounter = 0;

    private void Interact(InputAction.CallbackContext ctx)
    {

        if (currentInteractable != null && 
        currentInteractable.CanInteract() && 
        Time.frameCount != lastInteractionFrame)
        {
            // store last interaction frame so we aren't interacting multiple times in the same frame
            lastInteractionFrame = Time.frameCount;
            currentInteractable.Interact();

            interactCounter++;
            //Debug.Log($"Interact if statement has been run {interactCounter} times");

            //Debug.Log("Interacted with " + currentInteractable.GetInteractableType() + $"at time: {Time.time}, Frame: {Time.frameCount}");
        }
    }
}
