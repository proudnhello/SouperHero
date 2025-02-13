using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;

public class Charger : EnemyBaseClass
{
    private NavMeshAgent agent;
    protected new void Start(){
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }
    protected override void UpdateAI(){
        agent.SetDestination(playerTransform.position);
    }

    protected new void Update(){
        base.Update();
    }
}
