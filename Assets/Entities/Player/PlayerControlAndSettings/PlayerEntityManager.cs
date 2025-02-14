using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using static UnityEditor.Progress;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class PlayerEntityManager : Entity
{
    public static PlayerEntityManager Singleton { get; private set; }

    public PlayerHealth health;
    public new PlayerRenderer renderer;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;

        health = new(this);
        InitEntity();
    }

    #region OVERRIDE METHODS
    public override void AddHealth(int amount)
    {
        base.AddHealth(amount);
        health.ChangeHealth();     
    }
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        health.TakeDamage();
        health.ChangeHealth();
    }    
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is with an enemy and if the player is not invincible
        if (collision.gameObject.tag == "Enemy" && 
            collision.gameObject.layer != PlayerInputAndAttackManager.Singleton.interactableLayer &&
            !health.invincible)
        {
            TakeDamage(collision.gameObject.GetComponent<EnemyBaseClass>().playerCollisionDamage);
        }
    }

}
