using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBaseClass : Entity
{
    [Header("Enemy Info")]
    [SerializeField] protected Collectable ingredient;
    public int playerCollisionDamage = 10;

    [Header("Player Detection")]
    private bool playerDetected = false;
    private float detectionRadius = 4f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] LayerMask interactableLayer;

    internal SpriteRenderer _sprite;
    protected Transform _playerTransform;
    protected Color _initialColor;
    protected Collider2D _collider;
    protected NavMeshAgent agent;

    protected void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _playerTransform = PlayerEntityManager.Singleton.gameObject.transform;
        _initialColor = _sprite.color;
        _collider = GetComponent<Collider2D>();
        entityRenderer = new(this);
        agent = GetComponent<NavMeshAgent>();

        StartCoroutine(DetectionCoroutine());
        InitEntity();
    }

    protected void Update()
    {
        if (playerDetected)
        {
            UpdateAI();
        }
        else Patrol();
    }
    protected abstract void UpdateAI();
    protected void Die()
    {
        _sprite.color = _sprite.color / 1.5f;
        _collider.enabled = false;
        GameObject drop = Instantiate(ingredient.gameObject);
    }

    public override void ModifyHealth(int amount)
    {
        base.ModifyHealth(amount);
        if (IsDead())
        {
            Die();
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
