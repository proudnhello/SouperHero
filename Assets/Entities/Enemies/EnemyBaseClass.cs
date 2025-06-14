// This file was edited with help from GitHub Copilot

/*
 * This file was modified with LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log04.md
 */

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
using FMOD.Studio;
using FMODUnity;

public abstract class EnemyBaseClass : Entity
{
    [Header("Enemy Info")]
    [SerializeField] protected Collectable collectable;
    public int playerCollisionDamage = 10;
    [SerializeField] LayerMask interactableLayer;

    public static event Action<EnemyBaseClass> EnemyDamageEvent;

    internal SpriteRenderer _sprite;
    public Transform _playerTransform;
    protected Color _initialColor;
    protected Collider2D _collider;
    protected NavMeshAgent agent;
    protected EnemyAudio enemyAudio;
    public StudioEventEmitter enemyEmitter;

    // Used for boss fights to make the enemy always attack and never idle
    protected bool alwaysAggro = false;

    private bool hasDied = false;
    protected int enemyIndex;

    // TODO: make this override InitEntity and call base.InitEntity. Who did it this way?
    protected void initEnemy()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _playerTransform = PlayerEntityManager.Singleton.gameObject.transform;
        _initialColor = _sprite.color;
        _collider = GetComponent<Collider2D>();
        entityRenderer = new(this);
        agent = GetComponent<NavMeshAgent>();
        enemyEmitter = gameObject.AddComponent<StudioEventEmitter>();

        InitEntity();
    }
    protected virtual void UpdateAI() { }
    protected virtual void Die()
    {
        Die(true);
    }
    protected virtual void Die(bool drop)
    {
        AudioManager.Singleton._MusicHandler.RemoveAgro(enemyIndex);
        _sprite.color = _sprite.color / 1.5f;
        _collider.enabled = false;
        _rigidbody.velocity = Vector2.zero;
        agent.updatePosition = false;
        if (drop)
        {
            Instantiate(collectable.gameObject, transform.position, Quaternion.identity).GetComponent<Collectable>().Spawn(transform.position); //Spawn collectable on enemy death
        }
        entityRenderer.EnemyDeath();
        RunStateManager.Singleton.TrackEnemyDeath(enemyIndex);
    }

    public override void ApplyInfliction(List<FinishedSoup.SoupInfliction> spoonInflictions, Transform source, bool quiet = false)
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

        // Play enemy damage sfx
        if (!quiet)
        {
            EnemyDamageEvent?.Invoke(this);
        }

        base.ApplyInfliction(spoonInflictions, source, quiet);
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
            MetricsTracker.Singleton.RecordEnemyKilled();
            Die();
        }
    }

    public void SetIndex(int index)
    {
        enemyIndex = index;
    }

    // Helper function to sent the enemy after the player when they should be aware of them (ie boss fight)
    public void AttackPlayer()
    {
        alwaysAggro = true;
    }

    // Instant death for falling enemies
    // Respawn is for players only :)
    public override void Fall(Transform _respawnPoint)
    {
        GetComponent<Collider2D>().enabled = false;
        SetMoveSpeed(0);
        SetHealth(0);
        falling = true;
        agent.updatePosition = false;
        // THIS IS BAD AND I SHOULD NOT DO IT
        // But stupid knockback keeps messing everything up and the enemy's about to die anyway so it's probably fine
        // I can't wait for us to add an enemy that doesn't die when it falls and this breaks everything
        StopAllCoroutines(); 
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        StartCoroutine(Fall(_respawnPoint, 0.05f));
    }

    public IEnumerator Fall(Transform respawnPoint, float fallSpeed)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Vector3 initialScale = sprite.size;
        Vector2 changeAmount = new Vector2(initialScale.x / 10, initialScale.y / 10);

        while (sprite.size.x > 0)
        {
            yield return new WaitForSeconds(0.05f);
            sprite.size -= changeAmount;

        }

        Die(false);
    }
}
