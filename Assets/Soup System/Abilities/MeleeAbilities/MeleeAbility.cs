using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Melee")]
public class MeleeAbility : AbilityAbstractClass
{

    [Header("Melee")]
    //[SerializeField] ProjectileSpawner spawner;
    [SerializeField] float SIZE_MULTIPLIER = 2f;
    [SerializeField] float CRIT_MULTIPLIER = 0.5f;
    [SerializeField] float SPEED_MULTIPLIER = 0.5f;
    [SerializeField] float ATTACK_RATE_MULTIPLIER = 0.5f;
    private float size = 1;
    public int segments = 36;
    [SerializeField] AbilityStats stats;
    [SerializeField] List<Infliction> inflictions;

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {

        Debug.Log("Use Melee Ability");

        Debug.Log($"Enemy Layer: {CollisionLayers.Singleton.GetEnemyLayer().value}");
        Debug.Log($"Destroyable Layer: {CollisionLayers.Singleton.GetDestroyableLayer().value}");

        Vector2 currentDirection = PlayerEntityManager.Singleton.playerMovement.currentDirection.normalized;

        Vector2 center = new Vector2(PlayerEntityManager.Singleton.playerAttackPoint.position.x, PlayerEntityManager.Singleton.playerAttackPoint.position.y) + (currentDirection * SIZE_MULTIPLIER);

        // visualizes the attack area
        PlayerEntityManager.Singleton.StartCoroutine(SetCircle(center));

        // Get all colliders in range for enemies and destroyables
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(
            center,
            size * SIZE_MULTIPLIER,
            CollisionLayers.Singleton.GetEnemyLayer() | CollisionLayers.Singleton.GetDestroyableLayer()
        );

        Debug.Log("Hit Objects: " + string.Join(", ", hitObjects.Select(hit => hit.name)));

        Debug.Log(hitObjects);

        Debug.Log("Length of hit Objects: " + hitObjects.Length);

        foreach (Collider2D hitObject in hitObjects)
        {
            Vector3 directionToObject = hitObject.transform.position - PlayerEntityManager.Singleton.playerAttackPoint.position;
            float distanceToObject = directionToObject.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(PlayerEntityManager.Singleton.playerAttackPoint.position, directionToObject.normalized, distanceToObject, CollisionLayers.Singleton.GetEnvironmentLayer());

            // Check if the object is blocked by an obstacle
            if (hit.collider == null)
            {
                Entity entity = hitObject.GetComponent<Entity>();
                if (entity != null)
                {
                    // Apply the infliction to the enemy
                    entity.ApplyInfliction(inflictions, PlayerEntityManager.Singleton.playerAttackPoint);
                    continue;
                }

                Destroyables destroyableObject = hitObject.GetComponent<Destroyables>();
                Debug.Log($"Destroyable Object: {destroyableObject}");
                if (destroyableObject != null)
                {
                    // Remove the destroyable object
                    destroyableObject.RemoveDestroyable();
                }
            }
            else
            {
                // Log the name of the object hit by the ray
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
                Debug.Log("There is something collideable in the way!");
            }
        }
    }

    // Function to set the circle's size and position to match the ability's physics sphere
    private IEnumerator SetCircle(Vector2 center)
    {

        Debug.Log("Setting Attack UI");
        GameObject circle = PlayerEntityManager.Singleton.circle;
        circle.SetActive(true);

        if (circle != null)
        {
            // Set the position of the circle to match the player's attack point
            circle.transform.position = center;

            // Set the size of the circle (assuming the circle is a 2D object with a SpriteRenderer)
            float circleRadius = size;  // The radius of the circle matches the attack radius

            // Adjust the scale of the circle based on the size multiplier
            circle.transform.localScale = new Vector3(circleRadius * SIZE_MULTIPLIER, circleRadius * SIZE_MULTIPLIER, 1f);
        }
        else
        {
            Debug.LogWarning("Circle GameObject is not assigned.");
        }

        yield return new WaitForSeconds(.1f);

        circle.SetActive(false);
    }

}
