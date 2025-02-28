using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

// To create a projectile scriptable object, just go to the project overview, right click, click "create", and
// at the top should be a menu titled "abilities". Should be under that menu.
[CreateAssetMenu(menuName = "Abilities/DoubleProjectile")]
public class DoubleProjectile : AbilityAbstractClass
{
    [Header("Projectile")]
    [SerializeField] ProjectileSpawner spawner;
    [SerializeField] float SIZE_MULTIPLIER = 0.5f;
    [SerializeField] float CRIT_MULTIPLIER = 0.5f;
    [SerializeField] float SPEED_MULTIPLIER = 0.5f;

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {

        //Debug.Log($"Is spawner null?: {spawner}");
        // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
        ProjectileObject proj1 = spawner.GetProjectile();
        stats.size *= SIZE_MULTIPLIER;
        stats.speed *= SPEED_MULTIPLIER;
        Vector2 currDir = PlayerEntityManager.Singleton.playerAttackPoint.transform.up;
        float targetAngle = Mathf.Atan2(currDir.x, currDir.y) - .35f;
        Vector2 dir = new Vector2(currDir.x * Mathf.Cos(targetAngle) - currDir.y * Mathf.Sin(targetAngle),
            currDir.x * Mathf.Sin(targetAngle) + currDir.y * Mathf.Cos(targetAngle));
        proj1.Spawn(PlayerEntityManager.Singleton.playerAttackPoint.position,
            dir, stats, inflictions);
        //ProjectileObject proj2 = spawner.GetProjectile();
        //proj2.Spawn(PlayerEntityManager.Singleton.playerAttackPoint.position,
        //    PlayerEntityManager.Singleton.playerAttackPoint.transform.up,
        //    stats, inflictions);
        //proj1.transform.Rotate(new Vector3(20f, 0f, 0f));
        //proj2.transform.Rotate(new Vector3(160f, 0f, 0f));
    }
}
