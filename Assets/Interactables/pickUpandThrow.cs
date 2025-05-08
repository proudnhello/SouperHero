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
    //public SpriteRenderer thesprite;
    //int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    public GameObject playerHands;


    void Awake()
    {
        attack = PlayerEntityManager.Singleton.input.Player.UseSpoon;
        playerHands = GameObject.Find("/Player/AttackPointSwivel");
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
        Debug.Log("fire");
        if (!pickUp)
        {
            pickUpItem();
        }
        else
        {
            dropItem();
        }
        pickUp = !pickUp;

    }

    private void pickUpItem()
    {
        Debug.Log("pick up item");
        Vector3 oldPos = transform.position;
        transform.SetParent(playerHands.transform);
        Vector3 newPos = new Vector3(0, 0, oldPos.z);
        transform.localPosition = newPos;
    }

    private void dropItem()
    {

        Debug.Log("Drop item");
    }

    private void throwItem()
    {
        Debug.Log("throw item");
    }
}
