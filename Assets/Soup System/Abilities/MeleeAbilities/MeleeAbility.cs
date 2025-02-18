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

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {
        Debug.Log("Use Melee Ability");
        //Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
    }

    
}
