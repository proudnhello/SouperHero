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

    // Initialize and End are mostly for use by buffs, most abilities get triggered with a call to Active().
    public abstract void UseAbility(AbilityStats stats, List<Infliction> inflictions);
}
