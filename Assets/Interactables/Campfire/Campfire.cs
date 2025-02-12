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

        if (CanInteract() && cooking == false)
        {
            Cook();
        } 
        else if (CanInteract() && cooking == true)
        {
            StopCooking();
        }
    }

    private void Cook()
    {
        PlayerManager.instance.InventoryItems("flavor", true);
        PlayerManager.instance.InventoryItems("ability");
        CursorManager.Singleton.ShowCursor();
        cookingPot.SetActive(true);
        CookingScreen.SetActive(true);
        SetPlayerMovement(false);
        cooking = true;
    }

    private void StopCooking()
    {
        CursorManager.Singleton.HideCursor();
        SetPlayerMovement(true);
        cookingPot.SetActive(false);
        CookingScreen.SetActive(false);
        cooking = false;
    }

    private void SetPlayerMovement(bool value){
        GameObject player = GameObject.FindWithTag("Player"); // More efficient lookup


        if (player == null)
        {
            return;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        movement.enabled = value;
    }

    private void Update()
    {

    }
}
