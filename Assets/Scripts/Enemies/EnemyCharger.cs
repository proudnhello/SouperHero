using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class EnemyCharger : EnemyBaseClass
{
    [SerializeField]
    private int newMaxHealth = 100;
    [SerializeField]
    private float newMoveSpeed = 1f;
    [SerializeField]
    private String newSoupAbility = "charge";
    [SerializeField]
    private int newSoupNumber = 10;
    protected new void Start(){
        base.Start();
        maxHealth = newMaxHealth;
        moveSpeed = newMoveSpeed;
        currentHealth = maxHealth;
        soupAbility = newSoupAbility;
        soupNumber = newSoupNumber;
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
