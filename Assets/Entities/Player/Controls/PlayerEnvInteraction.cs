using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEnvInteraction : MonoBehaviour
{
    private Interactable currentInteractable = null;
    List<Interactable> withinRange;

    private void Start()
    {
        PlayerEntityManager.Singleton.input.Player.Interact.started += Interact;
        withinRange = new();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && interactable.CanInteract())
        {
            withinRange.Add(interactable);
            ChangeInteractableListContents();
        }
    }

    void ChangeInteractableListContents()
    {
        if (withinRange.Count == 0) currentInteractable = null;
        else if (withinRange.Count == 1) {
            currentInteractable = withinRange[0];
            currentInteractable.SetHighlighted(true);
        } else
        {
            float closestDist = Mathf.Infinity;
            int closestIndex = 0;
            for (int i = 0; i < withinRange.Count; i++)
            {
                withinRange[0].SetHighlighted(false);
                float newDist = Vector2.Distance(withinRange[i].transform.position, PlayerEntityManager.Singleton.transform.position);
                if (newDist < closestDist)
                {
                    closestDist = newDist;
                    closestIndex = i;
                }
            }
            currentInteractable = withinRange[closestIndex];
            currentInteractable.SetHighlighted(true);
        }
    }

    // continuously check to see what's the closest interactable
    private void Update()
    {
        foreach (var item in withinRange)
        {
            if (item == currentInteractable) continue;

            if (Vector2.Distance(item.transform.position, PlayerEntityManager.Singleton.transform.position) <
                Vector2.Distance(currentInteractable.transform.position, PlayerEntityManager.Singleton.transform.position))
            {
                currentInteractable.SetHighlighted(false);
                currentInteractable = item;
                currentInteractable.SetHighlighted(true);
                return;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && withinRange.Contains(interactable))
        {
            interactable.SetHighlighted(false);
            withinRange.Remove(interactable);
            if (currentInteractable == interactable) currentInteractable = null;
            ChangeInteractableListContents();
        }
    }


    private int lastInteractionFrame = -1;
    private void Interact(InputAction.CallbackContext ctx)
    {
        //handle dropping the item;
        if (PlayerInventory.Singleton.playerHolding)
        {
            //Debug.Log("I should place down object (Inside playerEnvInteraction");
            //DROPPING pickup Object on ground in front of you

            //Debug.Log(PlayerInventory.Singleton.objectHolding);

            Throwable.dropItem(PlayerInventory.Singleton.objectHolding.gameObject);
        }

        if (currentInteractable != null && 
        Time.frameCount != lastInteractionFrame)
        {
            // store last interaction frame so we aren't interacting multiple times in the same frame
            lastInteractionFrame = Time.frameCount;
            currentInteractable.Interact();
            if (currentInteractable && !currentInteractable.CanInteract())
            {
                withinRange.Remove(currentInteractable);
                currentInteractable = null;
                ChangeInteractableListContents();
            }
        }
    }

    
}
