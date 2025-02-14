using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBaseClass : Entity
{
    protected bool soupable = false;
    protected bool takingDamage = false;
    internal SpriteRenderer sprite;
    protected Transform playerTransform;
    [SerializeField] protected AbilityIngredient ingredient; 
    protected Rigidbody2D _rigidbody;
    protected Color _initialColor;
    public int playerCollisionDamage = 10;
    [SerializeField] protected float knockBackTime = 0.01f;

    [Header("Player Detection")]
    protected bool playerDetected = false;
    [SerializeField] protected float detectionRadius = 4f;
    protected float detectionDelay = 0.3f;
    protected LayerMask playerLayermask;

    protected NavMeshAgent agent;

    protected void Start(){
        sprite = GetComponent<SpriteRenderer>();
        playerTransform = PlayerManager.Singleton.player.transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialColor = sprite.color;
        health = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        playerLayermask = LayerMask.GetMask("Player");
        StartCoroutine(DetectionCoroutine());
        InitializeStats();
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
        if(Input.GetKeyDown(KeyCode.G)){
            Debug.Log(Soupify());
        }
    }

    public bool getSoupable(){
        return soupable;
    }
    public float GetKnockBackTime()
    {
        return knockBackTime;
    }

    public override void SetMoveSpeed(float newSpeed)
    {
        base.SetMoveSpeed(newSpeed);
        agent.speed = newSpeed;
    }
    protected abstract void UpdateAI();
    protected void BecomeSoupable(){
        soupable = true;
        GetComponent<Collider2D>().isTrigger = true;
        sprite.color = sprite.color / 1.5f;
    }
    public override void TakeDamage(int amount, GameObject source){
        if (!takingDamage)
        {
            takingDamage = true;
            health = Math.Clamp(health - amount, 0, maxHealth);
            Debug.Log("final damage " + amount);
            if (health == 0)
            {
                BecomeSoupable();
            }
            else
            {
                Vector3 direction = (transform.position - source.transform.position).normalized;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(direction * 10  , ForceMode2D.Impulse);
                StartCoroutine("KnockBack", knockBackTime);
            }
        }
    }

    public void TakeDamage(int amount, GameObject source, float knockback)
    {
        if (!takingDamage)
        {
            takingDamage = true;
            health = Math.Clamp(health - amount, 0, maxHealth);
            if (health == 0)
            {
                BecomeSoupable();
            }
            else
            {
                Vector3 direction = (transform.position - source.transform.position).normalized;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(direction, ForceMode2D.Impulse);
                StartCoroutine("KnockBack", knockback);
            }
        }
    }

    public override void TakeDamage(int amount) {
        if (!takingDamage)
        {
            takingDamage = true;
            health = Math.Clamp(health - amount, 0, maxHealth);
            if (health == 0)
            {
                BecomeSoupable();
            }
            else
            {;
                StartCoroutine("KnockBack", knockBackTime);
            }
        }
    }

    public void DamagePlayer(int damage) {
        PlayerHealth playerHealth = PlayerManager.Singleton.player.GetComponent<PlayerHealth>();
        
        // Check if enemy is not soupable and player is not invincible
        if (!soupable && !playerHealth.IsInvincible()) {
            PlayerManager.Singleton.TakeDamage(damage, this.gameObject);
            playerHealth.StartCoroutine(playerHealth.TakeDamageAnimation());
        }
    }

    public IEnumerator KnockBack(float time)
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(time);
        sprite.color = _initialColor;
        takingDamage = false;
        _rigidbody.velocity = Vector3.zero;
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
        Collider2D collider = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayermask);
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
