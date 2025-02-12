using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Campfire : Interactable
{
    [Header("Campfire")]
    [SerializeField] private GameObject cookingPot; 
    [SerializeField] private GameObject CookingScreen; 
    private float cookSpeed = 8f;
    private bool cooking = false;

    // Start is called before the first frame update
    private void Start()
    {
        type = this.name;
        SetInteractable(true);
        SetInteractablePrompt(false);
        cookingPot.SetActive(false);
    }

    public override void Interact()
    {
        Debug.Log($"[Frame {Time.frameCount}] Entering Interact() - Cooking: {cooking}, CanInteract: {CanInteract()}");
        //Debug.Log($"Cooking in set Interact: {cooking}");


        if (CanInteract() && cooking == false)
        {
            Debug.Log($"[Frame {Time.frameCount}] Inside if condition - About to start cooking");

            cookingPot.SetActive(true);
            CookingScreen.SetActive(true);
            Debug.Log("Setting player movement to false from Interact()");
            SetPlayerMovement(false);
            cooking = true;
        } else if (CanInteract() && cooking == true)
            {

                Debug.Log($"[Frame {Time.frameCount}] Inside if condition - About to end cooking");
                StopCooking();
        }

        Debug.Log($"[Frame {Time.frameCount}] Exiting Interact() - Final cooking state: {cooking}");
    }

    private void StopCooking()
    {
        Debug.Log("Setting player movement to true from StopCooking()");
        SetPlayerMovement(true);
        cookingPot.SetActive(false);
        CookingScreen.SetActive(false);
        cooking = false;
    }

    private void SetPlayerMovement(bool value){
        GameObject player = GameObject.FindWithTag("Player"); // More efficient lookup


        if (player == null)
        {
            Debug.LogWarning("No Player object found!");
            return;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            Debug.LogWarning("PlayerMovement component missing on Player!");
            return;
        }

        movement.enabled = value;
        Debug.Log($"Player movement set to {value}");
    }

    // ---- WIP ----
    // private void AnimatePotMovement()
    // {
    //     Debug.Log("Animating pot movement");
    //     cookingPot.SetActive(true);
    //     // animate the pot moving from the player to the campfire
    //     Vector2 playerPosition = GameObject.FindGameObjectsWithTag("Player")[0].transform.position;
    //     cookingPot.transform.position = playerPosition;
    //     cookingPot.transform.position = Vector2.MoveTowards(cookingPot.transform.position, this.transform.position, cookSpeed * Time.deltaTime);
    // }

    private void Update()
    {

        //if (!interactablePrompt.activeSelf)
        //{
        //    cookingPot.SetActive(false);
        //    CookingScreen.SetActive(false);
        //    cooking = false;
        //}


    }
}
