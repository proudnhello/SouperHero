using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

[CreateAssetMenu(menuName = "Abilities/Shield")]

public class Shield : AbilityAbstractClass
{
    [SerializeField] GameObject shieldPrefab;
    GameObject currentShield;

    protected override void Press(AbilityStats stats, List<Infliction> inflictions)
    {
        if (currentShield == null)
        {
            currentShield = Instantiate(shieldPrefab, PlayerEntityManager.Singleton.transform.position, Quaternion.identity);
            currentShield.transform.SetParent(PlayerEntityManager.Singleton.transform);
            currentShield.transform.localPosition = Vector3.zero;
        }

        // b/c this deals with player damage and inflictions on touch, like charge it needs to be handled in the PlayerEntityManager
        PlayerEntityManager.Singleton.SetShield(stats, inflictions, currentShield);
    }
}
