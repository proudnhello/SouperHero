using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseClass : MonoBehaviour
{
    private bool soupable = false;
    private int maxHealth = 100;
    private int currentHealth = 100;
    private SpriteRenderer sprite;
    public Transform playerTransform;
    private String soupAbility = "test";
    private int soupNumber = 1;
    void Start(){
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update(){
        UpdateAI();
        if(Input.GetKeyDown(KeyCode.F)){
            TakeDamage(50);
        }
        if(Input.GetKeyDown(KeyCode.G)){
            Debug.Log(Soupify());
        }
    }

    public int getCurrentHealth(){
        return currentHealth;
    }

    public bool getSoupable(){
        return soupable;
    }
    // FILL IN LATER, should move the enemy
    private void UpdateAI(){
        return;
    }
    private void BecomeSoupable(){
        soupable = true;
        sprite.color = new Color(255, 255, 255);
    }
    public void TakeDamage(int amount){
        currentHealth = Math.Clamp(currentHealth-amount, 0, maxHealth);
        if(currentHealth == 0){
            BecomeSoupable();
        }
    }

    public (String, int) Soupify(){
        if(soupable){
            Destroy(gameObject);
            return (soupAbility, soupNumber);
        }
        else{
            return ("null", -1);
        }
    }
}
