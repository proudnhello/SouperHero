using System;
using UnityEngine;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;


public class PlayerEntityManager : Entity
{
    public static PlayerEntityManager Singleton { get; private set; }

    public static event Action HealthChange;
    public PlayerInputActions input;
    public Transform playerAttackPoint;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        
        entityRenderer = new PlayerRenderer(this);
        InitEntity();
        input = new();
        input.Enable();
    }
    public override void ModifyHealth(int amount)
    {
        base.ModifyHealth(amount);
        if (amount < 0)
        {
            // check if the player is still alive if so, go to game over screen
            if (IsDead())
            {
                GameManager.instance.DeathScreen();
                return;
            }
        }
        HealthChange?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is with an enemy and if the player is not invincible
        if (collision.gameObject.tag == "Enemy" && 
            CollisionLayers.Singleton.InEnemyLayer(collision.gameObject) &&
            !inflictionHandler.IsAfflicted(InflictionType.SPIKY_Damage))
        {
            inflictionHandler.ApplyContactDamageInfliction(collision.gameObject.GetComponent<EnemyBaseClass>().playerCollisionDamage);
        }
    }

}
