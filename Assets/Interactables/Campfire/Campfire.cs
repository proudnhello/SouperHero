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
        if (CanInteract())
        {
            cooking = true;
            cookingPot.SetActive(true);
            CookingScreen.SetActive(true);
            Debug.Log("Setting player movement to false from Interact()");
            SetPlayerMovement(false);
            SetInteractable(false);
        }
    }

    private void SetPlayerMovement(bool value){
        //Debug.Log("Setting player movement to " + value);
        GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerMovement>().enabled = value;
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

        if (cooking && Input.GetKeyDown(PlayerManager.instance.interactionKey))
        {
            StopCooking();
        }

        if (!interactablePrompt.activeSelf)
        {
            cookingPot.SetActive(false);
            CookingScreen.SetActive(false);
            cooking = false;
        }


    }

    private void StopCooking(){
        Debug.Log("Setting player movement to true from StopCooking()");
        SetPlayerMovement(true);
        cookingPot.SetActive(false);
        CookingScreen.SetActive(false);
        SetInteractable(true);
        cooking = false;
    }
}
