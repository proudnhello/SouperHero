using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
//using static UnityEditor.Progress;
using Infliction = SoupSpoon.SpoonInfliction;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;

public class Throwable : Interactable
{
    // Start is called before the first frame update

    //public SpriteRenderer thesprite;
    //int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    internal GameObject playerHands;

    public static Transform dropSpot;

    public static Transform prevParent;

    Collider2D _Collider;

    [SerializeField] int THROW_DISTANCE = 6;
    [SerializeField] float THROW_TIME = 2f;
    [SerializeField] List<InflictionFlavor> InflictionFlavorsOnThrowContact;
    List<Infliction> inflictionsOnThrowContact = new();
    void Start()
    {
        _Collider = GetComponent<Collider2D>();
        _Collider.isTrigger = false;
        SetHighlighted(false);

        //Player hands above the head
        playerHands = PlayerEntityManager.Singleton.playerHoldingPoint;
        dropSpot = PlayerEntityManager.Singleton.playerAttackPoint;

        foreach (var inflictionFlavor in InflictionFlavorsOnThrowContact)
        {
            Infliction throwableInfliction = new(inflictionFlavor);
            throwableInfliction.AddIngredient(inflictionFlavor);
            inflictionsOnThrowContact.Add(throwableInfliction);
        }

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
            SetInteractable(false);  //Cannot interact multiple times
            SetHighlighted(false);
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
            PlayerInventory.Singleton.objectHolding = this;
            PlayerInventory.Singleton.playerHolding = true;

            _Collider.enabled = false;
        }
    }

    public static void dropItem(GameObject objectToDrop)
    {
        if (PlayerInventory.Singleton.playerHolding)
        {

            objectToDrop.GetComponent<Interactable>().SetInteractable(true); 
            //Debug.Log("Drop item");
            Transform needToDrop = objectToDrop.transform;

            needToDrop.SetParent(dropSpot);
            needToDrop.localPosition = new Vector3(0, .5f, needToDrop.position.z);
            needToDrop.SetParent(prevParent);

            PlayerInventory.Singleton.playerHolding = false;
            objectToDrop.GetComponent<Collider2D>().enabled = true;

            //Debug.Log("Success in dropping");
        }
    }

    bool hasContacted = false;
    public void ThrowItem(Vector2 throwPoint, Vector2 direction)
    {
        _Collider.enabled = true;
        _Collider.isTrigger = true;
        transform.parent = null;
        hasContacted = false;

        Vector2 startPoint = transform.position;
        Vector2 endPoint = new Vector2(direction.x * THROW_DISTANCE + throwPoint.x, direction.y * THROW_DISTANCE + throwPoint.y);

        StartCoroutine(HandleThrow());

        IEnumerator HandleThrow()
        {
            float time = 0;
            while (!hasContacted && time < THROW_TIME)
            {
                transform.position = Vector3.Lerp(startPoint, endPoint, time/THROW_TIME);
                yield return null;
                time += Time.deltaTime;
            }
            if (!hasContacted) OnThrowContact();
        }   
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasContacted) return;

        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject) && collider.tag != "PitHazard")
        {
            hasContacted = true;
            OnThrowContact();
        }
        else if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject) || (collider.gameObject.CompareTag("Player")))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            if (entity == null)
            {
                return;
            }

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictionsOnThrowContact, gameObject.transform);
            hasContacted = true;
            OnThrowContact();
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.TryGetComponent(out Entity destroyableEntity);
            if (destroyableEntity != null) // if it's an entity like a cactus, hurt it
            {
                destroyableEntity?.ApplyInfliction(inflictionsOnThrowContact, gameObject.transform);
            }
            else
            {
                collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
            }

            hasContacted = true;
            OnThrowContact();
        }
    }

    public virtual void OnThrowContact()
    {
        TryGetComponent(out Destroyables destroyable);
        destroyable?.RemoveDestroyable();
    }


    void OnDestroy()
    {
        if (this.gameObject == PlayerInventory.Singleton.objectHolding)
        {
            PlayerInventory.Singleton.objectHolding = null;
            PlayerInventory.Singleton.playerHolding = false;
        }
        
    }

}
