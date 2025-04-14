using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAndThrow : Interactable
{

    public GameObject holdSpot;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Interact()
    {
        Debug.Log("I just interacted in pick up and thrwo");
        //if I have an item, i need to drop it.
        if (itemHolding)
        {
            Debug.Log("Dropp item");
        }
        //else i dont have an item and i gotta pick it up. 
        else
        {
            Debug.Log("I pick up item now please");
        }
    }
}
