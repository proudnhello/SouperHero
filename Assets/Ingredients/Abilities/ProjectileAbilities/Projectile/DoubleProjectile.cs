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

    private float offset = 0.2f;

    //Get a projectile based on the direction a player is facing,
    //face the projectile in the direction of the target angle and spawn it
    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {

        stats.size *= SIZE_MULTIPLIER;
        stats.speed *= SPEED_MULTIPLIER;

        Vector2 currDir = PlayerEntityManager.Singleton.playerAttackPoint.transform.up;

        //First projectile
        ProjectileObject proj1 = spawner.GetProjectile();
        float targetAngle = Mathf.Atan2(currDir.y, currDir.x) - offset;
        Vector2 newDir = new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
        proj1.Spawn(PlayerEntityManager.Singleton.playerAttackPoint.position,
            newDir, stats, inflictions);

        //Second projectile
        ProjectileObject proj2 = spawner.GetProjectile();
        targetAngle = Mathf.Atan2(currDir.y, currDir.x) + offset;
        newDir = new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
        proj2.Spawn(PlayerEntityManager.Singleton.playerAttackPoint.position,
            newDir, stats, inflictions);
    }
}
