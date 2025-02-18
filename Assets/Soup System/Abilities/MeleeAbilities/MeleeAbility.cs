using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Melee")]
public class MeleeAbility : AbilityAbstractClass
{

    [Header("Melee")]
    //[SerializeField] ProjectileSpawner spawner;
    [SerializeField] float SIZE_MULTIPLIER = 0.5f;
    [SerializeField] float CRIT_MULTIPLIER = 0.5f;
    [SerializeField] float SPEED_MULTIPLIER = 0.5f;
    [SerializeField] float ATTACK_RATE_MULTIPLIER = 0.5f;
    private float size = 1;
    public int segments = 36;
    [SerializeField] AbilityStats stats;
    [SerializeField] List<Infliction> inflictions;
    //[SerializeField] GameObject circle;

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {
        //SetCircle();

        //circle.SetActive(true);

        Debug.Log("Use Melee Ability");

        // Get all colliders in range for enemies and destroyables
        Collider[] hitObjects = Physics.OverlapSphere(
            PlayerEntityManager.Singleton.playerAttackPoint.position,
            size,
            CollisionLayers.Singleton.GetEnemyLayer() | CollisionLayers.Singleton.GetDestroyableLayer()
        );

        Debug.Log(hitObjects);

        foreach (Collider hitObject in hitObjects)
        {
            Vector3 directionToObject = hitObject.transform.position - PlayerEntityManager.Singleton.playerAttackPoint.position;
            float distanceToObject = directionToObject.magnitude;

            // Check if the object is blocked by an obstacle
            if (!Physics.Raycast(PlayerEntityManager.Singleton.playerAttackPoint.position, directionToObject.normalized, distanceToObject, CollisionLayers.Singleton.GetCollisionLayer()))
            {
                Entity entity = hitObject.GetComponent<Entity>();
                if (entity != null)
                {
                    // Apply the infliction to the enemy
                    entity.ApplyInfliction(inflictions, PlayerEntityManager.Singleton.playerAttackPoint);
                    continue;
                }

                Destroyables destroyableObject = hitObject.GetComponent<Destroyables>();
                if (destroyableObject != null)
                {
                    // Remove the destroyable object
                    destroyableObject.RemoveDestroyable();
                }
            }
        }

        //circle.SetActive(false);
    }

    //// Function to set the circle's size and position to match the ability's physics sphere
    //private void SetCircle()
    //{
    //    if (circle != null)
    //    {
    //        // Set the position of the circle to match the player's attack point
    //        circle.transform.position = PlayerEntityManager.Singleton.playerAttackPoint.position;

    //        // Set the size of the circle (assuming the circle is a 2D object with a SpriteRenderer)
    //        float circleRadius = size;  // The radius of the circle matches the attack radius

    //        // Adjust the scale of the circle based on the size multiplier
    //        circle.transform.localScale = new Vector3(circleRadius * SIZE_MULTIPLIER, circleRadius * SIZE_MULTIPLIER, 1f);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Circle GameObject is not assigned.");
    //    }
    //}

}
