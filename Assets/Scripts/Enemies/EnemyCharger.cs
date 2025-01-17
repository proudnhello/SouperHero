using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class EnemyCharger : EnemyBaseClass
{
    protected new String soupAbility = "charge";
    protected new int soupNumber = 1;
    protected new void UpdateAI(){
        Vector2 direction = playerTransform.position - transform.position;
        direction = direction.normalized;
        direction *= Time.deltaTime;
        direction *= moveSpeed;
        transform.Translate(direction);
    }

    void Update(){
        if(!soupable){
            UpdateAI();
        }
        if(Input.GetKeyDown(KeyCode.F)){
            TakeDamage(50);
        }
        if(Input.GetKeyDown(KeyCode.G)){
            Debug.Log(Soupify());
        }
    }
}
