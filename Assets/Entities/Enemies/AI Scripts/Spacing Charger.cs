using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpacingCharger : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float detectionRadius = 9f;
    [SerializeField] private float followingRadius = 12f;
    [SerializeField] private float attackingRadius = 3f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] private float timeToAttack = 0.5f;
    [SerializeField] private float timeAfterAttack = 0.3f;
    [SerializeField] private float timeBetweenAttacks = 2.0f;
    [SerializeField] private float lungeDistance = 2.0f;
    private Animator animator;

    private enum SpacingChargerState
    {
        IDLING,
        CHASING,
        PREPARING,
        ATTACKING
    }
    private SpacingChargerState _state;

    protected void Start()
    {
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        _state = SpacingChargerState.IDLING;
        animator = GetComponent<Animator>();
        StartCoroutine(DetectionCoroutine());
    }

    protected void Update()
    {
        if (IsDead()) return;

        if (playerDetected)
        {
            UpdateAI();
        }

        switch(_state){
            case SpacingChargerState.IDLING:
                animator.Play("Idle");
                break;
            case SpacingChargerState.CHASING:
                animator.Play("Walk");
                break;
            case SpacingChargerState.PREPARING:
                animator.Play("Walk");
                break;
            case SpacingChargerState.ATTACKING:
                animator.Play("Attack");
                break;
        }
    }
    protected override void UpdateAI()
    {
        if (!agent.enabled)
        {
            return;
        }
        float distance = Vector2.Distance(_playerTransform.position, transform.position);
        if(distance < followingRadius){
            agent.SetDestination(_playerTransform.position);
            if(_state != SpacingChargerState.ATTACKING && _state != SpacingChargerState.PREPARING){
                StartCoroutine(ChasingCoroutine());
            }
        }
        else{
            _state = SpacingChargerState.IDLING;
        }

        if(_state != SpacingChargerState.ATTACKING){
            if(agent.destination.x > transform.position.x){
                _sprite.flipX = true;
            }
            else{
                _sprite.flipX = false;
            }   
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
        Collider2D collider;
        if(playerDetected){
            collider = Physics2D.OverlapCircle((Vector2)transform.position, followingRadius, playerLayer);
        }
        else{
            collider = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
        }
        if (collider != null)
        {
            playerDetected = true;
        } else
        {
            playerDetected = false;
        }
    }

    IEnumerator ChasingCoroutine(){
        // Wait a few seconds before attacking
        _state = SpacingChargerState.PREPARING;
        yield return new WaitForSeconds(timeBetweenAttacks);

        // Play attacking animation and set state
        _state = SpacingChargerState.ATTACKING;
        agent.speed *= 3f;

        // wait til animation is finshed   
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);
        agent.speed /= 3f;

        // clean up
        agent.isStopped = false;
        _state = SpacingChargerState.CHASING;
    }
}
