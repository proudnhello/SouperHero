using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;
using Infliction = SoupSpoon.SpoonInfliction;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;

public class RollingCactus : Entity
{
    [SerializeField] Sprite[] BurstAnimFrames;
    [SerializeField] float BurstFPS;
    [SerializeField] int FRAME_TO_RESET_ORIENTION = 3;
    [SerializeField] Collider2D _Collider;

    [SerializeField] List<InflictionFlavor> inflictionFlavors;
    List<Infliction> inflictionsOnContact = new();

    [SerializeField] float RollMoveSpeed;
    [SerializeField] float RollRotateSpeed;

    SpriteRenderer _SpriteRenderer;
    private void Start()
    {
        InitEntity();
        entityRenderer = new EntityRenderer(this);
        _SpriteRenderer = GetComponent<SpriteRenderer>();
        _Collider.isTrigger = false;
        foreach (var inflictionFlavor in inflictionFlavors)
        {
            Infliction cactusInfliction = new(inflictionFlavor);
            cactusInfliction.AddIngredient(inflictionFlavor);
            inflictionsOnContact.Add(cactusInfliction);
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
            }
        }
        if (IsDead())
        {
            StartCoroutine(Roll(source));
        }
    }

    bool hasContacted, canBreak;
    private IEnumerator Roll(Transform source)
    {
        _Collider.isTrigger = true;
        hasContacted = false;
        canBreak = true;

        // Calculate the angle between the tree parent and the damage source
        Vector3 direction = (transform.position - source.position).normalized;

        while (!hasContacted)
        {
            transform.localPosition += direction * RollMoveSpeed; 
            transform.RotateAround(transform.position, Vector3.forward, RollRotateSpeed * Mathf.Rad2Deg);
            yield return null;
        }

        // Rotate the parent object to face away from the damage source
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log(collider.name);
        if (!canBreak) return;

        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject) && collider.tag != "PitHazard")
        {
            hasContacted = true;
            gameObject.SetActive(false);
        }
        else if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject) || (collider.gameObject.CompareTag("Player")))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            if (entity == null)
            {
                return;
            }

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictionsOnContact, gameObject.transform);
            hasContacted = true;
            StartCoroutine(Burst());
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
            hasContacted = true;
            StartCoroutine(Burst());
        }
    }

    IEnumerator Burst()
    {
        for (int frame = 0; frame < BurstAnimFrames.Length; frame++)
        {
            if (frame == FRAME_TO_RESET_ORIENTION)
            {
                transform.localRotation = Quaternion.identity;
            }
            _SpriteRenderer.sprite = BurstAnimFrames[frame];
            yield return new WaitForSeconds(1/BurstFPS);
        }
        gameObject.SetActive(false);
    }
}
