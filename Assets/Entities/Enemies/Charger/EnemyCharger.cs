using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class EnemyCharger : EnemyBaseClass
{
    [SerializeField]
    protected new void Start(){
        base.Start();
        health = maxHealth;
    }
    protected override void UpdateAI(){
        Vector2 direction = playerTransform.position - transform.position;
        direction = direction.normalized;
        direction *= moveSpeed;
        _rigidbody.velocity = direction;
    }

    protected new void Update(){
        base.Update();
    }
}
