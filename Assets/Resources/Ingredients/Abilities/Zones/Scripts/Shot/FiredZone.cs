// portions of this file were generated using GitHub Copilot
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Infliction = SoupSpoon.SpoonInfliction;

// To create a projectile scriptable object, just go to the project overview, right click, click "create", and
// at the top should be a menu titled "abilities". Should be under that menu.
[CreateAssetMenu(menuName = "Abilities/FiredZone")]
public class FiredZone : AbilityAbstractClass
{
    [Header("Projectile")]
    [SerializeField] ZoneSpawner spawner;
    [SerializeField] float SIZE_MULTIPLIER = 0.5f;
    //[SerializeField] float CRIT_MULTIPLIER = 0.5f;
    [SerializeField] float SPEED_MULTIPLIER = 0.5f;

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {
        // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
        ZoneCore proj = spawner.GetProjectile();
        stats.size *= SIZE_MULTIPLIER;
        stats.speed *= SPEED_MULTIPLIER;

        Vector2 currentDirection = PlayerEntityManager.Singleton.playerMovement.currentDirection;
        
        //proj.transform.localScale = new Vector3(stats.size, stats.size, stats.size);
        SpriteRenderer spriteRenderer = proj.GetZoneArea().GetComponent<SpriteRenderer>();
        //Vector2 actualSize = new Vector2(rt.rect.width * rt.lossyScale.x, rt.rect.height * rt.lossyScale.y);
        float radius = spriteRenderer.sprite.bounds.extents.x * stats.size / 2;

        Vector2 center = new Vector2(PlayerEntityManager.Singleton.playerAttackPoint.position.x, PlayerEntityManager.Singleton.playerAttackPoint.position.y) + (radius * currentDirection);

        proj.Spawn(center,
            PlayerEntityManager.Singleton.playerAttackPoint.transform.up,
            stats, inflictions, false, null);
    }

}