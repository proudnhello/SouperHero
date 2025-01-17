using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseClass : MonoBehaviour
{
    protected bool soupable = false;
    protected int maxHealth = 100;
    protected int currentHealth = 100;
    protected float moveSpeed = 1f;
    protected SpriteRenderer sprite;
    protected Transform playerTransform;
    protected String soupAbility = "null";
    protected int soupNumber = -1;
    protected void Start(){
        sprite = GetComponent<SpriteRenderer>();
        playerTransform = PlayerManager.instance.player.transform;
    }

    protected void Update(){
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

    public int getCurrentHealth(){
        return currentHealth;
    }

    public bool getSoupable(){
        return soupable;
    }
    protected abstract void UpdateAI();
    protected void BecomeSoupable(){
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
