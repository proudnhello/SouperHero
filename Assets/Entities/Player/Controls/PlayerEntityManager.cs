// portions of this file were generated using GitHub Copilot
using System;
using System.Collections.Generic;
using UnityEngine;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;
using Infliction = FinishedSoup.SoupInflictionStat;
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
    [SerializeField] bool DEBUG_INVINCIBLE;
    public Transform currentPlayerLocation;
    SpriteRenderer spriteRenderer;
    [SerializeField] GameObject miniMapIcon;
    [SerializeField] Collider2D _collider;
    [SerializeField] Rigidbody2D rb;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;

        InitEntity();
        input = new();
        input.Enable();
        entityRenderer = new PlayerRenderer(this, animations);
        GameManager.OnEndRun += GameEnd;
    }

    private void Start()
    {
        playerAudio = new PlayerAudio();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = PlayerCosmeticRenderer.Singleton.playerMaterial;
    }

    void GameEnd(bool s)
    {
        currentPlayerLocation.position = new Vector3(99999999f, 99999999f, 0);
        input.Disable();
        StopAllCoroutines();
        if (shieldObject != null) shieldObject.SetActive(false);
        spriteRenderer.enabled = false;
        miniMapIcon.SetActive(false);
        _collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
    }

    private void OnDisable()
    {
        input.Disable();
        ((PlayerRenderer)entityRenderer).Disable();
        GameManager.OnEndRun -= GameEnd;
    }
    public override void ModifyHealth(int amount)
    {
#if UNITY_EDITOR
        if (DEBUG_INVINCIBLE) return;
#endif

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
        return currentPlayerLocation.position;
    }

    public override bool IsInvincible()
    {
        return playerMovement.charging || shielded;
    }

    public override void DealDamage(int damage)
    {
        if (!GameManager.Singleton.GameRunning) return;
        // If we're charging, don't apply inflictions
        if (!IsInvincible())
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

        shieldDurationCoroutine = StartCoroutine(ShieldDuration(stats.ModifiedDuration));
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

    public override void ApplyInfliction(List<Infliction> spoonInflictions, Transform source)
    {
        base.ApplyInfliction(spoonInflictions, source);

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
        playerMovement.StartCoroutine(playerMovement.Charge(stats.ModifiedDuration, stats.ModifiedSpeed));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Singleton.GameRunning) return;

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
        if (!GameManager.Singleton.GameRunning) return;
        _collider.enabled = false;
        SetMoveSpeed(11, 0);
        StartCoroutine(FallAnimation(respawnPoint));
    }

    IEnumerator FallAnimation(Transform respawnPoint)
    {
        Vector3 initialScale = spriteRenderer.size;
        Vector2 changeAmount = new Vector2(initialScale.x / 10, initialScale.y / 10);

        while (spriteRenderer.size.x > 0)
        {
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.size -= changeAmount;

        }

        spriteRenderer.size = initialScale;
        ResetMoveSpeed(11);
        DealDamage(GetBaseStats().maxHealth / 9); // Deal damage to the player == 1/9 of max health or one heart
        transform.position = respawnPoint.position;
        _collider.enabled = true;
    }
}