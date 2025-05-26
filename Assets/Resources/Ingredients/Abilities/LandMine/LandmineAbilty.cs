using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Landmine")]
public class Landmine : AbilityAbstractClass
{
    [SerializeField] LandmineObject landminePrefab;
    LandmineObject currentMine;

    protected override void Press(AbilityStats stats, List<Infliction> inflictions)
    {
        Vector2 vector2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMine = Instantiate(landminePrefab, vector2, Quaternion.identity);
        currentMine.init(stats.size);
    }

    protected override void Hold(AbilityStats stats, List<Infliction> inflictions)
    {
        Vector2 vector2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMine.transform.position = vector2;
    }

    protected override void Release(AbilityStats stats, List<Infliction> inflictions)
    {
        // Trim the z axis to 0, since we are using 2D.
        currentMine.StartCoroutine(currentMine.Detonate(stats.duration, stats.size, inflictions) ); 
    }
}
