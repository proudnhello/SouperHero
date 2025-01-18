using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base ability abstract class. Contains declarations for functions that abilities will implement
public abstract class AbilityAbstractClass : ScriptableObject
{
    [Header("Info")]
    [SerializeField] public string _abilityName;

    [Header("Usage")]
    [SerializeField] protected float _maxUsage;
    [SerializeField] protected float _remainingUsage;

    // Initialize and End are mostly for use by buffs, most abilities get triggered with a call to Active().
    public abstract void Initialize(int duration);
    public abstract void Active();
    public abstract void End();
}
