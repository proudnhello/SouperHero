using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    protected abstract void UpdateAI();
    protected void Die()
    {
        _sprite.color = _sprite.color / 1.5f;
        _collider.enabled = false;
        _rigidbody.velocity = Vector2.zero;
        Instantiate(collectable.gameObject, transform.position, Quaternion.identity).GetComponent<Collectable>().Spawn(transform.position); //Spawn collectable on enemy death
        StartCoroutine(entityRenderer.EnemyDeathAnimation());
    }

    public override void ModifyHealth(int amount)
    {
        base.ModifyHealth(amount);
        if (IsDead())
        {
            Die();
        }
    }
}
