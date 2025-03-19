using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

// To create a projectile scriptable object, just go to the project overview, right click, click "create", and
// at the top should be a menu titled "abilities". Should be under that menu.
[CreateAssetMenu(menuName = "Abilities/PlayerCenteredZone")]
public class PlayerCenteredZone : AbilityAbstractClass
{
    [Header("Projectile")]
    [SerializeField] ZoneSpawner spawner;
    [SerializeField] float SIZE_MULTIPLIER = 0.5f;
    //[SerializeField] float CRIT_MULTIPLIER = 0.5f;
    [SerializeField] float SPEED_MULTIPLIER = 0.5f;
    bool active = false;

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {
        Debug.Log($"Is spawner null?: {spawner}");
        // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
        ZoneCore proj = spawner.GetProjectile();
        stats.size *= SIZE_MULTIPLIER;
        stats.speed *= SPEED_MULTIPLIER;
        proj.Spawn(PlayerEntityManager.Singleton.playerAttackPoint.position,
            PlayerEntityManager.Singleton.playerAttackPoint.transform.up,
            stats, inflictions, true, this);
        active = true;
    }
}