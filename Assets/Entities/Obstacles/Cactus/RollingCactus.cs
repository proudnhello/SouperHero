using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;

public class RollingCactus : Entity
{
    [SerializeField] Sprite[] BurstAnimFrames;
    [SerializeField] float BurstFPS;
    [SerializeField] int FRAME_TO_RESET_ORIENTION = 3;
    [SerializeField] Collider2D _Collider;
    [SerializeField] Destroyables destroyable;

    [SerializeField] List<InflictionFlavor> inflictionOnRollFlavors;
    [SerializeField] int KNOCKBACK_INDEX = 1;
    [SerializeField] Vector2 MinMaxDamageToSpeed = new Vector2(5, 40);
    List<Infliction> inflictionsOnRollContact = new();

    [SerializeField] int PLAYER_COLLISION_DAMAGE = 5;

    SpriteRenderer _SpriteRenderer;
    private void Start()
    {
        InitEntity();
        entityRenderer = new EntityRenderer(this);
        _SpriteRenderer = GetComponent<SpriteRenderer>();
        _Collider.isTrigger = false;
        foreach (var inflictionFlavor in inflictionOnRollFlavors)
        {
            Infliction cactusInfliction = new(inflictionFlavor);
            cactusInfliction.AddIngredient(inflictionFlavor);
            inflictionsOnRollContact.Add(cactusInfliction);
        }     
    }

    public override void Fall(Transform _respawnPoint)
    {
        gameObject.SetActive(false);
        return; 
    }

    // There's a load of code when it comes to inflictions that assumes we have agents or rigidbodies. The humble tree does not.
    // So, you can only damage it directly. This is a bit of a hack, but it works.
    public override void ApplyInfliction(List<Infliction> spoonInflictions, Transform source, bool quiet = false)
    {
        foreach (Infliction infliction in spoonInflictions)
        {
            if (infliction.InflictionFlavor.inflictionType == InflictionFlavor.InflictionType.SPIKY_Damage)
            {
                DealDamage((int)infliction.amount);
                if (IsDead() && !hasContacted)
                {
                    if (IRoll != null) StopCoroutine(IRoll);
                    StartCoroutine(IRoll = Roll(source, (int)infliction.amount));
                }
            }
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is with an enemy and if the player is not invincible
        if (collision.gameObject.CompareTag("Player"))
        {
            Entity player = collision.gameObject.GetComponent<Entity>();
            player?.DealDamage(PLAYER_COLLISION_DAMAGE);
        }
    }

    bool hasContacted = false, canBreak = false;
    IEnumerator IRoll;
    private IEnumerator Roll(Transform source, int damage)
    {
        _Collider.isTrigger = true;
        hasContacted = false;
        canBreak = true;

        // use attack damage to calculate cactus speed + knockback
        damage = (int)Mathf.Clamp(damage, MinMaxDamageToSpeed.x, MinMaxDamageToSpeed.y);
        float RollMoveSpeed = .4f * damage + 2;
        float RollRotateSpeed = RollMoveSpeed * 2;
        inflictionsOnRollContact[KNOCKBACK_INDEX].add = damage/2;

        // Calculate the angle between the tree parent and the damage source
        Vector3 direction = (transform.position - source.position).normalized;
        int sign = direction.x < 0 ? 1 : -1;

        while (!hasContacted)
        {
            transform.localPosition += direction * RollMoveSpeed * Time.deltaTime; 
            transform.RotateAround(transform.position, Vector3.forward, sign * RollRotateSpeed * Mathf.Rad2Deg * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!canBreak || hasContacted) return;

        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject) && collider.tag != "PitHazard")
        {
            hasContacted = true;
            StartCoroutine(Burst());
        }
        else if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject) || (collider.gameObject.CompareTag("Player")))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            if (entity == null)
            {
                return;
            }

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictionsOnRollContact, gameObject.transform);
            hasContacted = true;
            StartCoroutine(Burst());
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.TryGetComponent(out Entity destroyableEntity);
            if (destroyableEntity != null) // if it's an entity like a cactus, hurt it
            {
                destroyableEntity?.ApplyInfliction(inflictionsOnRollContact, gameObject.transform);
            } else
            {
                collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
            }

            hasContacted = true;
            StartCoroutine(Burst());
        }
    }

    IEnumerator Burst()
    {
        _Collider.enabled = false;
        for (int frame = 0; frame < BurstAnimFrames.Length; frame++)
        {
            if (frame == FRAME_TO_RESET_ORIENTION)
            {
                transform.localRotation = Quaternion.identity;
            }
            _SpriteRenderer.sprite = BurstAnimFrames[frame];
            yield return new WaitForSeconds(1/BurstFPS);
        }
        destroyable.ManualDestroy();
        Destroy(gameObject);
    }
}
