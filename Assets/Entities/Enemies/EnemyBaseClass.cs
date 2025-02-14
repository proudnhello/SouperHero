using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask interactableLayer;

    protected bool soupable = false;
    protected bool takingDamage = false;

    internal SpriteRenderer _sprite;
    protected Transform _playerTransform;
    protected Rigidbody2D _rigidbody;
    protected Color _initialColor;
    protected Collider2D _collider;

    protected void Start(){
        _sprite = GetComponent<SpriteRenderer>();
        _playerTransform = PlayerEntityManager.Singleton.gameObject.transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialColor = _sprite.color;
        _collider = GetComponent<Collider2D>();

        StartCoroutine(DetectionCoroutine());
        InitEntity();
    }

    protected void Update(){
        if(!soupable && !takingDamage)
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
        gameObject.layer = interactableLayer;
        _sprite.color = _sprite.color / 1.5f;
    }

    #region OVERRIDE METHODS

    public override void TakeDamage(int damage, GameObject source, float knockback)
    {
        if (!takingDamage)
        {
            takingDamage = true;
            base.TakeDamage(damage);
            if (GetHealth() <= 0)
            {
                BecomeSoupable();
            }
            else
            {
                Vector3 direction = (transform.position - source.transform.position).normalized;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(direction * knockback, ForceMode2D.Impulse);
                StartCoroutine("KnockBack");
            }
        }
    }

    public override void TakeDamage(int damage) {
        if (!takingDamage)
        {
            takingDamage = true;
            base.TakeDamage(damage);
            if (GetHealth() <= 0)
            {
                BecomeSoupable();
            }
            else
            {;
                StartCoroutine("KnockBack");
            }
        }
    }

    #endregion


    public IEnumerator KnockBack()
    {
        int maxFlashCycles = Mathf.CeilToInt((invincibility / 0.3f));
        int flashCycles = 0;
        while(maxFlashCycles > flashCycles)
        {
            _sprite.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            _sprite.color = _initialColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
        takingDamage = false;
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
