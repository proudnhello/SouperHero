using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;

public class Spacer : EnemyBaseClass
{
    private NavMeshAgent agent;
    [SerializeField] float attackRadius = 2.0f;
    [SerializeField] float timeBetweenAttacks = 1.0f;
    private bool playerWithinRange;
    protected new void Start(){
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        playerWithinRange = false;
    }
    protected override void UpdateAI(){
        Collider2D collider = Physics2D.OverlapCircle((Vector2)transform.position, attackRadius, playerLayermask);
        if(collider != null){
            playerWithinRange = true;
        }   
        else{
            playerWithinRange = false;
        }
        Debug.Log(playerWithinRange);
        if(!playerWithinRange){
            agent.isStopped = false;
        }
        else{
            agent.isStopped = true;
        }
        agent.SetDestination(playerTransform.position);
    }

    protected new void Update(){
        base.Update();
    }
}
