using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnvInteraction : MonoBehaviour
{
    private Interactable currentInteractable = null;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && interactable.CanInteract())
        {
            currentInteractable = interactable;
            interactable.SetInteractablePrompt(true);
            Debug.Log("Can interact with " + currentInteractable.GetInteractableType());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
            interactable.SetInteractablePrompt(false);
            Debug.Log("Can no longer interact with " + interactable.GetInteractableType());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(PlayerManager.instance.interactionKey))
        {
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                currentInteractable.Interact();
                Debug.Log("Interacted with " + currentInteractable.GetInteractableType());
            }
        }
    }
}
