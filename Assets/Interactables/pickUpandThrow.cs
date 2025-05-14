using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class pickUpandThrow : Interactable
{
    // Start is called before the first frame update
    
    //public SpriteRenderer thesprite;
    //int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    public GameObject playerHands;

    public static GameObject dropSpot;

    public static Transform prevParent;


    void Awake()
    {
        //Player hands above the head
        playerHands = GameObject.Find("/Player/Hands");

        //dropSpot is 
        dropSpot = GameObject.Find("/Player/AttackPointSwivel/AttackPoint");
    }
    void Start()
    {
        // Debug.Log(CanInteract());
        // SetHighlighted(true);
        //thesprite = GetComponent<SpriteRenderer>();
        //Debug.Log(thesprite.sprite);
        //thesprite.material.SetFloat(_OutlineThickness, 1);
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public override void Interact()
    {
        pickUpItem();
       

    }

    private void pickUpItem()
    {
        if (!(PlayerInventory.Singleton.playerHolding))
        {

            //Debug.Log("pick up item");
            //get previous parent reference
            prevParent = transform.parent;
            //change parent to Hands in player gameobject
            transform.SetParent(playerHands.transform);
            //Get new position
            Vector3 newPos = new Vector3(0, 0, transform.position.z);

            //change local position of game object (i.e. barrel moves to above players head)
            transform.localPosition = newPos;

            //tells playerInventory the player is holding something
            PlayerInventory.Singleton.objectHolding = this.gameObject;
            PlayerInventory.Singleton.playerHolding = true;
        }
    }

    public static void dropItem(GameObject objectToDrop)
    {
        if (PlayerInventory.Singleton.playerHolding)
        {


            //Debug.Log("Drop item");
            Transform needToDrop = objectToDrop.transform;

            needToDrop.SetParent(dropSpot.transform);
            needToDrop.localPosition = new Vector3(0, .5f, needToDrop.position.z);
            needToDrop.SetParent(prevParent);

            PlayerInventory.Singleton.playerHolding = false;

            //Debug.Log("Success in dropping");
        }

        
    }

    
}
