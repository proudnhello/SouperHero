using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Charge")]

public class Charge : AbilityAbstractClass
{
    [SerializeField] float CRIT_MULTIPLIER = 0.5f;
    [SerializeField] float SPEED_MULTIPLIER = 100f;

    public override void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {
        PlayerEntityManager.Singleton.Charge(stats, inflictions);
    }
}
