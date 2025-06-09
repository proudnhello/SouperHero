// portions of this file were generated using GitHub Copilot
using System;
using System.Collections.Generic;
using UnityEngine;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;
using Infliction = FinishedSoup.SoupInfliction;
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
    public GameObject playerHoldingPoint;
    private bool cooked = false;
    private bool shielded = false;
    private GameObject shieldObject;
    private List<Infliction> shieldInflictions;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;

        InitEntity();
        input = new();
        input.Enable();
        entityRenderer = new PlayerRenderer(this, animations);
    }

    private void Start()
    {
        playerAudio = new PlayerAudio();
    }

    private void OnDisable()
    {
        input.Disable();
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
                GameManager.Singleton.EndRun(false);
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
        // If we're charging, don't apply inflictions
        if (!playerMovement.charging && !shielded)
        {
            base.DealDamage(damage);
        }

        // If we're shielded, remove it
        if (shielded)
        {
            StartCoroutine(RemoveShield());
        }
    }

    Coroutine shieldDurationCoroutine = null;

    public void SetShield(AbilityStats stats, List<Infliction> inflictions, GameObject shieldObject)
    {
        // If we have a shield, restart the timer
        if (shielded && shieldDurationCoroutine != null)
        {
            StopCoroutine(shieldDurationCoroutine);
        }

        shielded = true;
        shieldInflictions = inflictions;
        this.shieldObject = shieldObject;
        shieldObject.SetActive(true);

        shieldDurationCoroutine = StartCoroutine(ShieldDuration(stats.duration));
    }

    public IEnumerator RemoveShield()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to allow for inflictions to be applied before removing the shield
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
        shielded = false;
        StopCoroutine(shieldDurationCoroutine);
    }

    public IEnumerator ShieldDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(RemoveShield());
    }

    public override void ApplyInfliction(List<Infliction> spoonInflictions, Transform source, bool quiet = false)
    {
        // If we're charging, don't apply inflictions
        if (!playerMovement.charging && !shielded)
        {
            base.ApplyInfliction(spoonInflictions, source, quiet);
        }

        // If we're shielded, remove it
        if (shielded)
        {
            StartCoroutine(RemoveShield());
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
        // If we're charging or shielded and we hit an enemy, apply the inflictions
        if (CollisionLayers.Singleton.InEnemyLayer(collision.gameObject) && (playerMovement.charging || shielded))
        {
            Entity enemy = collision.gameObject.GetComponent<Entity>();
            if (enemy != null)
            {
                if (playerMovement.charging)
                {
                    enemy.ApplyInfliction(chargeInflictions, transform);
                }
                else if (shielded)
                {
                    enemy.ApplyInfliction(shieldInflictions, transform);
                    StartCoroutine(RemoveShield()); // Removing this line of code makes the player immortal while the shild is active. Could be fun powerup?
                }
            }
        }

        // If we're charging and we hit a destroyable, remove it. We don't if we're shielded cuz that feels weird
        if (CollisionLayers.Singleton.InDestroyableLayer(collision.gameObject) && playerMovement.charging)
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
        GetComponent<Collider2D>().enabled = false;
        SetMoveSpeed(0);
        StartCoroutine(FallAnimation(respawnPoint));
    }

    IEnumerator FallAnimation(Transform respawnPoint)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Vector3 initialScale = sprite.size;
        Vector2 changeAmount = new Vector2(initialScale.x / 10, initialScale.y / 10);

        while (sprite.size.x > 0)
        {
            yield return new WaitForSeconds(0.05f);
            sprite.size -= changeAmount;

        }

        sprite.size = initialScale;
        ResetMoveSpeed();
        DealDamage(GetBaseStats().maxHealth / 9); // Deal damage to the player == 1/9 of max health or one heart
        transform.position = respawnPoint.position;
        GetComponent<Collider2D>().enabled = true;
    }
}