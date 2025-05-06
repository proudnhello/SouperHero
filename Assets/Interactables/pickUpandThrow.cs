using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class pickUpandThrow : Interactable
{
    // Start is called before the first frame update
    private bool pickUp = false;
    InputAction attack;

    void Awake()
    {
        attack = PlayerEntityManager.Singleton.input.Player.UseSpoon;
    }
    void Start()
    {
        Debug.Log(CanInteract());
        SetHighlighted(true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public override void Interact(){
        Debug.Log("fire");
        if(!pickUp){
            pickUpItem();
        } else {
            dropItem();
        }
        pickUp = !pickUp;

    }

    private void pickUpItem(){
        Debug.Log("pick up item");
    }

    private void dropItem(){
        Debug.Log("Drop item");
    }

    private void throwItem(){
        Debug.Log("throw item");
    }
}
