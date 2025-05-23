using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

// Base ability abstract class. Contains declarations for functions that abilities will implement
public abstract class AbilityAbstractClass : ScriptableObject
{
    [Header("Info")]
    [SerializeField] public string _abilityName;

    protected virtual void Press(AbilityStats stats, List<Infliction> inflictions)
    {
        return;
    }

    protected virtual void Hold(AbilityStats stats, List<Infliction> inflictions)
    {
        return;
    }

    protected virtual void Release(AbilityStats stats, List<Infliction> inflictions)
    {
        return;
    }

    public void UseAbility(AbilityStats stats, List<Infliction> inflictions)
    {
        PlayerEntityManager.Singleton.StartCoroutine(AbilityCoroutine(stats, inflictions));
    }

    protected virtual IEnumerator AbilityCoroutine(AbilityStats stats, List<Infliction> inflictions)
    {
        Press(stats, inflictions);
        bool oneFrame = false;
        while(PlayerEntityManager.Singleton.input.Player.UseSpoon.inProgress == true || !oneFrame)
        {
            Hold(stats, inflictions);
            oneFrame = true;
            yield return null;
        }

        Release(stats, inflictions);
    }
}
