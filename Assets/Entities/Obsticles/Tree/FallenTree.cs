using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This file was edited using GitHub Copilot

public class FallenTree : Entity
{
    [SerializeField] float onFallDamageTimer = 0.01f;
    [SerializeField] float FallDamage = 50;
    bool dealingDamage = true;
    private void Start()
    {
        InitEntity();
        entityRenderer = new EntityRenderer(this);
    }

    public override void Fall(Transform _respawnPoint)
    {
        return; // The trees should not move, and they should not fall
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(!dealingDamage) return;

        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && entity != this)
        {
            entity.DealDamage((int)FallDamage);
        }
    }

    public void Fallen()
    {
        StartCoroutine(FallDamageCooldown());
    }

    IEnumerator FallDamageCooldown()
    {
        yield return new WaitForSeconds(onFallDamageTimer);
        GetComponent<Collider2D>().isTrigger = false;
        dealingDamage = false;
    }

    // There's a load of code when it comes to inflictions that assumes we have agents or rigidbodies. The humble tree does not.
    // So, you can only damage it directly. This is a bit of a hack, but it works.
    public override void ApplyInfliction(List<SoupSpoon.SpoonInfliction> spoonInflictions, Transform source, bool quiet = false)
    {
        foreach (SoupSpoon.SpoonInfliction infliction in spoonInflictions)
        {
            if (infliction.InflictionFlavor.inflictionType == FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage)
            {
                DealDamage((int)infliction.amount);
            }
        }
        if (IsDead())
        {
            FallOver();
        }
    }

    private void FallOver()
    {
        Destroy(gameObject);
    }
}
