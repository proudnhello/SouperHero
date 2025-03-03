using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Melee")]
public class MeleeAbility : AbilityAbstractClass
{

    [Header("Melee")]
    //[SerializeField] ProjectileSpawner spawner;
    [SerializeField] float SIZE_MULTIPLIER = 2f;
    [SerializeField, Range(2, 100)] int RayCastNum = 36;
    [SerializeField] float RayCastAngleRad = .4f;

    [Header("Melee Anim")]
    [SerializeField] Sprite[] MeleeVFXFrames;
    [SerializeField] AbilityVFXSpawner Spawner;

    public override void UseAbility(AbilityStats passedStats, List<Infliction> inflictions)
    {
        Debug.Log(Time.time + " used ability");
        Vector2 playerDir = PlayerEntityManager.Singleton.playerMovement.currentDirection.normalized;
        float playerAngle = Mathf.Atan2(playerDir.y, playerDir.x);

        HashSet<Collider2D> hitColliders = new();

        float length = passedStats.size * SIZE_MULTIPLIER;

        for (float angle = -RayCastAngleRad; angle <= RayCastAngleRad; angle += 2*RayCastAngleRad/(RayCastNum-1))
        {
            float targetAngle = playerAngle + angle;
            Vector2 rayDir = new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));

            RaycastHit2D wall = Physics2D.Raycast(PlayerEntityManager.Singleton.gameObject.transform.position,
                rayDir, length, CollisionLayers.Singleton.GetEnvironmentLayer());
            float rayLength = wall.collider != null ? Vector2.Distance(wall.point, PlayerEntityManager.Singleton.gameObject.transform.position) : length;
            RaycastHit2D[] hits = Physics2D.RaycastAll(PlayerEntityManager.Singleton.gameObject.transform.position,
                rayDir, rayLength, CollisionLayers.Singleton.GetEnemyLayer() | CollisionLayers.Singleton.GetDestroyableLayer());
            Debug.DrawRay(PlayerEntityManager.Singleton.gameObject.transform.position,
                rayDir * rayLength, Color.magenta, .5f);
            foreach (var hit in hits) hitColliders.Add(hit.collider);
        }

        foreach (var col in hitColliders)
        {
            Entity entity = col.GetComponent<Entity>();
            if (entity != null)
            {
                // Apply the infliction to the enemy
                entity.ApplyInfliction(inflictions, PlayerEntityManager.Singleton.playerAttackPoint);
                continue;
            }

            Destroyables destroyableObject = col.GetComponent<Destroyables>();
            Debug.Log($"Destroyable Object: {destroyableObject}");
            if (destroyableObject != null)
            {
                // Remove the destroyable object
                destroyableObject.RemoveDestroyable();
            }
        }
        PlayerEntityManager.Singleton.StartCoroutine(HandleAnim(length));
    }

    private IEnumerator HandleAnim(float rayLength)
    {
        SpriteRenderer ren = Spawner.GetMelee(PlayerEntityManager.Singleton.playerAttackPoint);
        ren.gameObject.SetActive(true);
        ren.flipX = PlayerEntityManager.Singleton.playerMovement.currentDirection.x > 0;
        float scale = Mathf.Clamp(.4875f * rayLength - .28f, .1f, 10f);
        ren.transform.parent.localScale = new Vector2(scale, scale);
        for (int frame = 0; frame < MeleeVFXFrames.Length; frame++)
        {
            ren.sprite = MeleeVFXFrames[frame];
            yield return new WaitForSeconds(1f / PlayerEntityManager.Singleton.animations.AttackFPS);
        }
        ren.gameObject.SetActive(false);

    }

}
