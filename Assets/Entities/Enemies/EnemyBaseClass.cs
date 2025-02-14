using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBaseClass : Entity
{
    [Header("Enemy Info")]
    [SerializeField] protected AbilityIngredient ingredient;
    [SerializeField] protected float knockBackTime = 1.0f;
    public int playerCollisionDamage = 10;

    [Header("Player Detection")]
    private bool playerDetected = false;
    private float detectionRadius = 4f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] LayerMask interactableLayer;

    protected bool soupable = false;

    internal SpriteRenderer _sprite;
    protected Transform _playerTransform;
    protected Color _initialColor;
    protected Collider2D _collider;
    protected NavMeshAgent agent;

    protected void Start(){
        _sprite = GetComponent<SpriteRenderer>();
        _playerTransform = PlayerEntityManager.Singleton.gameObject.transform;
        _initialColor = _sprite.color;
        _collider = GetComponent<Collider2D>();
        entityRenderer = new(this);

        StartCoroutine(DetectionCoroutine());
        InitEntity();
    }

    protected void Update(){
        if(!soupable)
        {
            if (playerDetected)
            {
                UpdateAI();
            }
            else Patrol();
        } else if (soupable)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = 0.0f;
        }
    }

    public bool getSoupable(){
        return soupable;
    }
    protected abstract void UpdateAI();
    protected void BecomeSoupable(){
        soupable = true;
        gameObject.layer = CollisionLayers.Singleton.GetInteractableLayer();
        _sprite.color = _sprite.color / 1.5f;
    }

    public override void ModifyHealth(int amount)
    {
        base.ModifyHealth(amount);
        if (amount < 0)
        {
            BecomeSoupable();
        }
    }

    public AbilityIngredient Soupify(){
        if(soupable){

            Debug.Log("Enemy Is Soupable in Soupify");
            Destroy(gameObject);
            return ingredient;
        }
        else{
            Debug.Log("Enemy Is Not Soupable in Soupify");
            AbilityIngredient nullIngredient = new AbilityIngredient();
            nullIngredient.name = "null";
            return nullIngredient;
        }
    }

    IEnumerator DetectionCoroutine()
    {
        yield return new WaitForSeconds(detectionDelay);
        CheckDetection();
        StartCoroutine(DetectionCoroutine());
    }

    protected void CheckDetection()
    {
        Collider2D collider = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
        if (collider != null)
        {
            playerDetected = true;
        } else
        {
            playerDetected = false;
        }
    }

    protected void Patrol()
    {
        _rigidbody.velocity = Vector2.zero;
    } 

    private void OnDrawGizmos() //Testing only
    {
        //Lo: Toggle Gizmos on in game view to see radius
        //Display detection radius on enemies
        Gizmos.color = new Color(255, 0, 0, 0.25f);
        if (playerDetected) Gizmos.color = new Color(0, 255, 0, 0.25f);
        Gizmos.DrawSphere((Vector2)transform.position, detectionRadius);
    }

}
