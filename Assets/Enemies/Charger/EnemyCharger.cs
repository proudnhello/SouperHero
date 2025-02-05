using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class EnemyCharger : EnemyBaseClass
{
    [SerializeField]
    private float newMoveSpeed = 1f;
    protected new void Start(){
        base.Start();
        moveSpeed = newMoveSpeed;
        currentHealth = maxHealth;
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
