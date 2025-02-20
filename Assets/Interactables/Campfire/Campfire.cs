using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Campfire : Interactable
{
    [Header("Campfire")]
    [SerializeField] private GameObject CookingScreen; 
    private float cookSpeed = 8f;
    private bool cooking = false;

    // Start is called before the first frame update
    private void Start()
    {
        SetInteractable(true);
        SetHighlighted(false);
    }

    public override void Interact()
    {

        if (CanInteract() && cooking == false)
        {
            Debug.Log("Enter Cooking");
            Cook();
        } 
        else if (CanInteract() && cooking == true)
        {
            Debug.Log("Exit Cooking");
            StopCooking();
        }
    }

    private void Cook()
    {
        CookingManager.Singleton.CurrentCampfire = this;
        CursorManager.Singleton.ShowCursor();
        if(CookingScreen == null)
        {
            CookingScreen = CookingManager.Singleton.CookingCanvas;
        }
        CookingScreen.SetActive(true);
        SetPlayerMovement(false);
        cooking = true;
        CookingManager.Singleton.ResetStatsText();
    }

    public void StopCooking()
    {
        CursorManager.Singleton.HideCursor();
        SetPlayerMovement(true);
        CookingScreen.SetActive(false);
        cooking = false;
        CookingManager.Singleton.ResetStatsText();
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
}
