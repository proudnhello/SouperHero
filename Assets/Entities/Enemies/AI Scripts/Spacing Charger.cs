using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;

public class SpacingCharger : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float followingRadius = 6f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] float attackRadius = 3.0f;
    [SerializeField] float timeBetweenAttacks = 1.0f;
    private bool playerWithinRange;
    protected void Start(){
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        playerWithinRange = false;
    }
    protected override void UpdateAI(){
        Collider2D collider = Physics2D.OverlapCircle((Vector2)transform.position, attackRadius, playerLayer);
        if(collider != null){
            playerWithinRange = true;
        }   
        else{
            playerWithinRange = false;
        }

        if(playerWithinRange){
            agent.isStopped = true;
        }
        else{
            agent.isStopped = false;
        }

        agent.SetDestination(_playerTransform.position);
    }

    protected void Update()
    {
        if (IsDead()) return;

        if (playerDetected)
        {
            UpdateAI();
        }
    }
}
