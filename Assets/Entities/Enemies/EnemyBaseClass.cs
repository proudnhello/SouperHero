using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static FlavorIngredient.InflictionFlavor;
using BuffFlavor = FlavorIngredient.BuffFlavor;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

public abstract class EnemyBaseClass : Entity
{
    [Header("Enemy Info")]
    [SerializeField] protected Collectable collectable;
    public int playerCollisionDamage = 10;
    [SerializeField] LayerMask interactableLayer;

    internal SpriteRenderer _sprite;
    protected Transform _playerTransform;
    protected Color _initialColor;
    protected Collider2D _collider;
    protected NavMeshAgent agent;
    private EnemySpawnLocation spawn;

    private bool hasDied = false;

    protected void initEnemy()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _playerTransform = PlayerEntityManager.Singleton.gameObject.transform;
        _initialColor = _sprite.color;
        _collider = GetComponent<Collider2D>();
        entityRenderer = new(this);
        agent = GetComponent<NavMeshAgent>();

        InitEntity();
    }
    protected virtual void UpdateAI() { }
    protected virtual void Die()
    {
        _sprite.color = _sprite.color / 1.5f;
        _collider.enabled = false;
        _rigidbody.velocity = Vector2.zero;
        agent.updatePosition = false;
        Instantiate(collectable.gameObject, transform.position, Quaternion.identity).GetComponent<Collectable>().Spawn(transform.position); //Spawn collectable on enemy death
        StartCoroutine(entityRenderer.EnemyDeathAnimation());
        if(spawn != null){
            spawn.enemy = null;
        }
    }

    public override void ApplyInfliction(List<SoupSpoon.SpoonInfliction> spoonInflictions, Transform source)
    {
        // If the enemy is not currently moving towards something, make them move towards the player
        // I'm so excited for this to break when we implement enemies taking damage from things other than the player
        // These need to be set to vector2s as the z axis is meaningless yet still exists
        if ((Vector2)agent.destination == (Vector2)transform.position)
        {
            agent.SetDestination(PlayerEntityManager.Singleton.GetPlayerPosition());
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                // Who the fuck at unity made *this* the way of checking if an animator has an animation?
                // Not that it works, anyway
                if (anim.HasState(anim.GetLayerIndex("Base Layer"), Animator.StringToHash("Walk"))){
                    anim.Play("Walk");
                }
            }
        }
        base.ApplyInfliction(spoonInflictions, source);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is with an enemy and if the player is not invincible
        if (playerCollisionDamage <= 0) return;
        if (collision.gameObject.tag == "Player")
        {
            Entity player = collision.gameObject.GetComponent<Entity>();
            player?.DealDamage(playerCollisionDamage);
        }
    }

    protected virtual void Update()
    {
        if (IsDead()) return;
        UpdateAI();
    }

    public override void ModifyHealth(int amount)
    {
        base.ModifyHealth(amount);
        if (IsDead() && !hasDied)
        {
            hasDied = true;
            Die();
        }
    }

    public void setSpawn(EnemySpawnLocation s){
        spawn = s;
    }
}
