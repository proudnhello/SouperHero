using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class EnemyCharger : EnemyBaseClass
{
    protected new void Start(){
        base.Start();
    }
    protected override void UpdateAI(){
        Vector2 direction = _playerTransform.position - transform.position;
        direction = direction.normalized;
        direction *= GetMoveSpeed();
        _rigidbody.velocity = direction;
    }

    protected new void Update(){
        base.Update();
    }
}
