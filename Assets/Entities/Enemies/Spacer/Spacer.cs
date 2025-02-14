using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;

public class Spacer : EnemyBaseClass
{
    [SerializeField] float attackRadius = 3.0f;
    [SerializeField] float timeBetweenAttacks = 1.0f;
    private bool playerWithinRange;
    protected new void Start(){
        base.Start();
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

        if(playerWithinRange){
            agent.isStopped = true;
        }
        else{
            agent.isStopped = false;
        }

        agent.SetDestination(playerTransform.position);
    }

    protected new void Update(){
        base.Update();
    }
}
