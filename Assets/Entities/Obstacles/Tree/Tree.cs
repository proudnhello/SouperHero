using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This file was edited using GitHub Copilot

public class Tree : Entity
{
    [SerializeField] GameObject fallenTree;
    private void Start()
    {
        InitEntity();
        entityRenderer = new EntityRenderer(this);
    }

    public override void Fall(Transform _respawnPoint)
    {
        return; // The trees should not move, and they should not fall
    }

    // There's a load of code when it comes to inflictions that assumes we have agents or rigidbodies. The humble tree does not.
    // So, you can only damage it directly. This is a bit of a hack, but it works.
    public override void ApplyInfliction(List<FinishedSoup.SoupInfliction> spoonInflictions, Transform source, bool quiet = false)
    {
        foreach (FinishedSoup.SoupInfliction infliction in spoonInflictions)
        {
            if (infliction.InflictionFlavor.inflictionType == FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage)
            {
                DealDamage((int)infliction.amount);
            }
        }
        if (IsDead())
        {
            FallOver(source);
        }
    }

    private void FallOver(Transform source)
    {
        fallenTree.SetActive(true);
        fallenTree.GetComponent<FallenTree>().Fallen();
        Transform parent = transform.parent;
        // Calculate the angle between the tree parent and the damage source
        Vector3 direction = (source.position - parent.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x);

        // Rotate the parent object to face away from the damage source
        parent.RotateAround(parent.position, Vector3.forward, angle * Mathf.Rad2Deg);

        this.gameObject.SetActive(false);
    }
}
