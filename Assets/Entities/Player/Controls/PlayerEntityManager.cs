// portions of this file were generated using GitHub Copilot
using System;
using System.Collections.Generic;
using UnityEngine;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;
using Infliction = SoupSpoon.SpoonInfliction;
using skner.DualGrid;
using System.Collections;

public class PlayerEntityManager : Entity
{
    public static PlayerEntityManager Singleton { get; private set; }

    public static event Action HealthChange;
    public PlayerInputActions input;
    public Transform playerAttackPoint;
    public PlayerAnimationHolder animations;
    public PlayerMovement playerMovement;
    public PlayerAudio playerAudio;
    private bool cooked = false;


    private void Awake()
    {
        if (Singleton == null) Singleton = this;
           
        InitEntity();
        input = new();
        input.Enable();
        //InputManager.playerInput.SwitchCurrentActionMap("Player");
        entityRenderer = new PlayerRenderer(this, animations);
    }

    private void Start()
    {
        playerAudio = new PlayerAudio();
    }

    private void OnDisable()
    {
        input.Disable();
        //InputManager.playerInput.SwitchCurrentActionMap("UI");
        ((PlayerRenderer)entityRenderer).Disable();
    }
    public override void ModifyHealth(int amount)
    {
        base.ModifyHealth(amount);
        if (amount < 0)
        {
            // check if the player is still alive if so, go to game over screen
            if (IsDead())
            {
                MetricsManager.Singleton.EndTimer();
                MetricsManager.Singleton.RecordNumDeaths();
                MetricsManager.Singleton.SaveToMetricsToSO();
                GameManager.instance.DeathScreen();
                return;
            }
        }
        HealthChange?.Invoke();
    }

    public bool HasCooked()
    {
        return cooked;
    }

    public void SetCooked(bool cook)
    {
        cooked = cook;
    }

    public Vector2 GetPlayerPosition()
    {
        return transform.position;
    }

    public override void DealDamage(int damage)
    {
        // If we're charging, don't take damage
        if (!playerMovement.charging)
        {
            base.DealDamage(damage);
        }
    }

    // Because charge is handled by collisions with the player, the damage has to be handled in the player entity
    // Annoying it can't be fit in the charge script, but ah well
    List<Infliction> chargeInflictions = new List<Infliction>();
    public void Charge(AbilityStats stats, List<Infliction> inflictions)
    {
        chargeInflictions = inflictions;
        playerMovement.StartCoroutine(playerMovement.Charge(stats.duration, stats.speed));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we're charging and we hit an enemy, apply the inflictions
        if (CollisionLayers.Singleton.InEnemyLayer(collision.gameObject) && playerMovement.charging)
        {
            Entity enemy = collision.gameObject.GetComponent<Entity>();
            if (enemy != null)
            {
                enemy.ApplyInfliction(chargeInflictions, transform);
            }
        }

        if(CollisionLayers.Singleton.InDestroyableLayer(collision.gameObject) && playerMovement.charging)
        {
            Destroyables destroyable = collision.gameObject.GetComponent<Destroyables>();
            if (destroyable != null)
            {
                destroyable.RemoveDestroyable();
            }
        }
    }

    public override void Fall(Transform respawnPoint)
    {
        // TODO: Add good fall animation 
        SetMoveSpeed(0);
        StartCoroutine(FallAnimation(respawnPoint));
    }

    IEnumerator FallAnimation(Transform respawnPoint)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Vector3 initialScale = sprite.size;
        Vector2 changeAmount = new Vector2(initialScale.x/10, initialScale.y/10); 

        while(sprite.size.x > 0)
        {
            yield return new WaitForSeconds(0.05f);
            sprite.size -= changeAmount;

        }

        sprite.size = initialScale;
        ResetMoveSpeed();
        DealDamage(GetBaseStats().maxHealth / 9); // Deal damage to the player == 1/9 of max health or one heart
        transform.position = respawnPoint.position;
    }
}
