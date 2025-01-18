using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/AbilityExample1")]
public class Ability1ScriptableObject : AbilityAbstractClass
{
    public override void Active()
    {
        Debug.Log("Ability1 Example Active");
    }

    public override void Initialize(int duration)
    {
        Debug.Log("Ability1 Example Initialize, value: " + duration);
    }

    public override void End()
    {
        Debug.Log("Ability1 Example End");
    }
}
