// portions of this file were generated using GitHub Copilot
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Charge")]

public class Charge : AbilityAbstractClass
{
    //[SerializeField] float CRIT_MULTIPLIER = 0.5f;
    //[SerializeField] float SPEED_MULTIPLIER = 100f;

    protected override void Release(AbilityStats stats, List<Infliction> inflictions)
    {
        PlayerEntityManager.Singleton.Charge(stats, inflictions);
    }
}
